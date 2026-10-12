# Plataforma de Jornada — D&D Group

Aplicação web para registrar e acompanhar a jornada de trabalho de colaboradores no Brasil e em outros países. O sistema reúne registro de ponto, cálculo do espelho mensal, pedidos de ajuste, consulta de equipes e ferramentas administrativas.

Este README descreve as funcionalidades, regras de negócio, estrutura, API e formas de execução que existem no repositório.

## Índice

- [Funcionalidades](#funcionalidades)
- [Tecnologias e arquitetura](#tecnologias-e-arquitetura)
- [Perfis e acesso](#perfis-e-acesso)
- [Regras de negócio](#regras-de-negócio)
- [Cadastro de funcionários pelo RH](#cadastro-de-funcionários-pelo-rh)
- [Modelo de dados](#modelo-de-dados)
- [Organização do código](#organização-do-código)
- [API HTTP](#api-http)
- [Executar com Docker Compose](#executar-com-docker-compose)
- [Executar em desenvolvimento local](#executar-em-desenvolvimento-local)
- [Build, testes e migrations](#build-testes-e-migrations)
- [Dados de demonstração](#dados-de-demonstração)
- [Escopo atual e observações](#escopo-atual-e-observações)

## Funcionalidades

### Colaborador

- Entrar e sair da aplicação, com renovação de sessão.
- Consultar o próprio perfil e o estado atual da jornada.
- Registrar entrada, início e fim de intervalo e saída, selecionando escritório, remoto ou viagem.
- Consultar marcações por intervalo de datas e acompanhar o espelho mensal.
- Solicitar inclusão, alteração ou anulação de uma marcação e acompanhar a avaliação.

### Gestor

Além das funções de colaborador:

- Consultar o estado atual dos membros da própria equipe.
- Abrir o espelho mensal de um membro da equipe.
- Aprovar ou rejeitar solicitações de ajuste dos subordinados diretos.

### RH

Além das funções de gestão:

- Consultar a lista de funcionários.
- Opcionalmente, cadastrar funcionários como colaborador ou gestor, escolhendo país, jornada, gestor e senha inicial.
- Fechar o período mensal de um país.
- Gerar e baixar relatório mensal em CSV, opcionalmente filtrado por país.

## Tecnologias e arquitetura

| Parte | Implementação |
|---|---|
| Interface | Angular 21, componentes standalone, TypeScript, Signals, formulários e SCSS |
| API | ASP.NET Core Web API no .NET 10 |
| Persistência | Entity Framework Core 10 e PostgreSQL 17 (provedor Npgsql) |
| Autenticação | JWT para acesso e tokens de renovação rotativos |
| Proxy e publicação web | Nginx no container do frontend |
| Ambiente completo | Docker Compose |

```text
Navegador
  └── Angular (localhost:4200)
        └── /api
              └── ASP.NET Core (localhost:5287)
                    └── Entity Framework Core
                          └── PostgreSQL (localhost:5433)
```

Em desenvolvimento, o servidor do Angular encaminha `/api` para `http://localhost:5287` usando `frontend/proxy.conf.json`. No Compose, o Nginx encaminha `/api/` para o serviço `api` na rede interna do Compose e serve a aplicação Angular, incluindo as rotas do cliente.

## Perfis e acesso

Os perfis reconhecidos são `Colaborador`, `Gestor` e `RH`.

| Tela | Acesso |
|---|---|
| `/login` | Público |
| `/alterar-senha-inicial` | Usuário autenticado cuja conta exige a troca da senha inicial |
| `/ponto` | Usuário autenticado |
| `/espelho` | Usuário autenticado; mostra o próprio espelho |
| `/ajustes` | Usuário autenticado; gestores e RH também podem avaliar solicitações autorizadas |
| `/equipe` | Gestor ou RH; o backend limita a consulta à equipe permitida |
| `/admin` | RH |

As rotas do Angular usam guards para controlar a navegação, mas a autorização efetiva também é aplicada pela API. Chamadas autenticadas enviam o token de acesso; diante de uma resposta `401`, o interceptor tenta renová-lo e, se a renovação falhar, encerra a sessão local e redireciona para o login. Contas criadas pelo RH precisam trocar a senha inicial antes de acessar as demais funções; essa restrição também é imposta pela API.

## Regras de negócio

### Registro de ponto

- Os tipos de marcação são `Entrada`, `InicioIntervalo`, `FimIntervalo` e `Saida`.
- Fora de uma jornada, a próxima marcação permitida é entrada. Durante o trabalho, o usuário pode iniciar intervalo ou registrar saída; durante o intervalo, deve registrar seu fim.
- Uma jornada com mais de 16 horas desde a última marcação é considerada abandonada para definir o estado atual, permitindo iniciar outra jornada.
- Cada marcação guarda o instante ocorrido em UTC, o instante de registro no servidor, o fuso horário, o offset UTC, a data local da jornada, a modalidade, a origem e, quando fornecida, uma chave de idempotência.
- A data de jornada é associada à entrada correspondente. Se não houver entrada anterior aplicável, é derivada da data local da marcação.
- Marcações comuns usam o horário do servidor. Marcações com origem `Offline` precisam informar o instante ocorrido e não podem estar mais de dois minutos no futuro.
- Reenvios com a mesma chave de idempotência retornam a marcação existente, sem duplicá-la.
- O backend valida a ordem cronológica e a sequência permitida. Marcações de origem `Ajuste` só são criadas pelo fluxo de aprovação de solicitação.

### Fusos e cálculo de jornada

- Os fusos são identificadores IANA (por exemplo, `America/Sao_Paulo` e `Europe/Lisbon`).
- Os intervalos trabalhados são calculados pela diferença entre instantes UTC, e não pela subtração de horários locais. O offset é preservado para apresentar o horário local original.
- A jornada pertence à data local da entrada. Essa associação mantém juntas as marcações de uma jornada que atravessa a meia-noite ou uma mudança de fuso.
- O espelho calcula minutos trabalhados, intervalo, saldo em relação à jornada contratual e a situação de cada dia. A resposta inclui os dias, as marcações válidas e os totais do mês.
- Dias de fim de semana, feriados, ausências, faltas, dias incompletos e dias ainda pendentes são classificados separadamente. Para jornadas com mais de seis horas trabalhadas, o espelho sinaliza intervalo abaixo do mínimo contratado.

### Como funciona o espelho mensal

O espelho é calculado pela API a partir da jornada contratual, das marcações válidas, dos feriados e das ausências cadastradas para o funcionário. A competência usa o formato `AAAA-MM`; a tela `/espelho` permite escolher o mês e apresenta uma linha para cada dia do calendário, além dos totais da competência.

Para cada dia, o backend:

1. Busca as marcações cuja `DataJornadaLocal` pertence àquele dia. A data da jornada é determinada pelo fuso local da entrada; as durações, por sua vez, são calculadas pela diferença entre instantes UTC.
2. Soma o tempo entre entrada e início do intervalo e entre fim do intervalo e saída. Os períodos de intervalo ficam separados do tempo trabalhado. Marcações anuladas não entram no cálculo.
3. Calcula os minutos esperados a partir da jornada contratual diária nos dias úteis sem feriado ou ausência. Fim de semana, feriado e ausência têm zero minuto esperado.
4. Calcula o saldo do dia como `minutos trabalhados - minutos esperados`. O total mensal soma minutos esperados, trabalhados e saldo.
5. Classifica a situação do dia:
   - `Regular`: existem marcações e a última marcação válida do dia é uma saída.
   - `Incompleto`: existem marcações, mas a jornada não termina com uma saída.
   - `Ausencia`: não há marcações e há uma ausência cadastrada para a data.
   - `Feriado`: não há marcações nem ausência, mas há feriado cadastrado para o país.
   - `FimDeSemana`: não há marcações, ausência ou feriado e a data é sábado ou domingo.
   - `Pendente`: é um dia útil sem marcações na data atual ou futura, ainda não considerado falta.
   - `Falta`: é um dia útil passado sem marcações, ausência ou feriado.

O resumo mensal inclui minutos esperados, trabalhados e saldo, além da contagem de dias incompletos e faltas. Cada dia também informa observação (feriado ou ausência, quando aplicável), minutos de intervalo e se o intervalo ficou abaixo do mínimo contratual. Essa sinalização é feita quando houve mais de seis horas trabalhadas e o intervalo computado foi inferior ao mínimo da jornada. O resultado identifica se o período está fechado; fechamentos são separados por país e competência.

O relatório CSV administrativo reutiliza os totais do espelho e contém, por funcionário ativo, minutos esperados, trabalhados, saldo, dias incompletos e faltas. Feriados e ausências são considerados apenas quando existem registros correspondentes no banco; o semeador de desenvolvimento fornece feriados de demonstração para 2026.

### Ajustes

- Os tipos de solicitação são `Adicionar`, `Alterar` e `Anular`; toda solicitação exige motivo.
- O pedido começa como `Pendente`. Apenas Gestor ou RH podem avaliá-lo, e somente solicitações pendentes podem ser avaliadas.
- Ao aprovar inclusão ou alteração, o serviço cria uma nova marcação com origem `Ajuste`. Ao alterar ou anular, a marcação alvo é anulada, preservando o registro histórico; a nova marcação, quando aplicável, referencia a substituída.
- Antes de salvar uma aprovação, a API valida a sequência cronológica resultante. Ajustes que deixariam horários duplicados ou uma sequência de tipos inválida são recusados.
- O acesso às solicitações é limitado pelo funcionário autenticado: colaborador consulta as próprias, gestor as que pode avaliar para a equipe e RH as solicitações autorizadas pelo serviço.

### Fechamento de período

- O fechamento é feito por competência (`AAAA-MM`) e código de país.
- Uma marcação ou ajuste que afetaria um período fechado é rejeitado pela API. O fechamento é associado ao país e à competência, e não globalmente ao mês de todos os países.
- Não existe no momento uma operação de reabertura exposta pela API.

## Cadastro de funcionários pelo RH

Na tela `/admin`, o RH pode, se precisar, criar um acesso como `Colaborador` ou `Gestor`. O cadastro é opcional: a tela de administração, a consulta de funcionários, o fechamento de período e a emissão de relatórios continuam disponíveis sem criar um novo usuário. O formulário começa fechado e as opções de país, jornada e gestor só são carregadas quando o RH escolhe abrir o cadastro. O perfil `RH` não pode ser atribuído por esse formulário. O cadastro solicita nome, e-mail, senha inicial, país do contrato, fuso horário padrão, jornada contratual e, opcionalmente, um gestor responsável.

Países, jornadas e gestores ativos disponíveis são carregados do banco para preencher os seletores. Ao escolher um país, o formulário sugere o fuso padrão cadastrado para ele; o RH pode ajustar o identificador IANA. A senha inicial deve ter de 8 a 128 caracteres. O backend normaliza o e-mail para minúsculas, rejeita e-mails já cadastrados, valida as referências de país, jornada, fuso e gestor e armazena somente o hash da senha. Funcionários novos ficam ativos. Após o cadastro, a lista da tela é atualizada.

A senha inicial não é enviada por e-mail nem retornada pela API: o RH deve entregá-la ao funcionário por um canal seguro. No primeiro login, a pessoa é encaminhada para `/alterar-senha-inicial` e precisa definir uma senha pessoal de pelo menos 12 caracteres antes de continuar. Enquanto a troca estiver pendente, os endpoints normais da API recusam o token; apenas a operação de troca fica autorizada. Ao concluir, os tokens de renovação existentes são revogados e a pessoa entra novamente com a nova senha. O projeto ainda não implementa redefinição de senha.

O formulário de cadastro é independente das outras operações da tela. Alertas de erro são exibidos quando uma chamada ao sistema falha; deixar o cadastro fechado ou simplesmente não adicionar um funcionário não gera erro.

## Modelo de dados

O contexto `PontoDbContext` mapeia as entidades abaixo. As configurações de tabelas, chaves, relacionamentos e índices ficam em `backend/Data/Configurations/`; a estrutura é versionada nas migrations de `backend/Data/Migrations/`.

| Entidade | Finalidade |
|---|---|
| `Pais` | Código do país, nome e fuso horário padrão. |
| `JornadaContratual` | Jornada diária e semanal e minutos mínimos de intervalo. |
| `Funcionario` | Conta, perfil, país, fuso padrão, jornada, gestor e situação ativa. |
| `MarcacaoPonto` | Registro de ponto, horário, fuso, origem, modalidade, idempotência e eventual anulação/substituição. |
| `SolicitacaoAjuste` | Pedido de inclusão, alteração ou anulação e seu fluxo de avaliação. |
| `Feriado` | Feriado cadastrado para um país e data. |
| `Ausencia` | Tipo, funcionário e período de ausência usados na classificação do espelho. |
| `FechamentoPeriodo` | Situação do fechamento por país e competência, com responsável e instante. |
| `TokenRenovacao` | Hash, validade e revogação dos tokens de renovação. |

Os enums de `backend/Models/Enums/` definem os valores de perfil, modalidade, origem, tipo de marcação, situação da jornada, tipo/status de ajuste, tipo de ausência e status de fechamento. As regras de sequência, competência, fuso e cálculo ficam em `backend/Domain/`.

## Organização do código

### Backend — `backend/`

- `Controllers/`: endpoints HTTP para autenticação, perfil próprio, marcações, espelhos, ajustes, equipe e administração.
- `Services/`: casos de uso e acesso coordenado aos dados, incluindo autenticação, registro, cálculo de espelho, avaliação de ajustes, equipe, fechamento e CSV.
- `Domain/`: tipos e regras de negócio independentes das telas, como sequência de marcação, cálculo de jornada, competência e fuso horário.
- `Models/` e `Models/Enums/`: entidades persistidas e valores enumerados.
- `Dtos/`: contratos de entrada e resposta da API.
- `Data/`: `PontoDbContext`, configurações EF Core, migrations e semeador de desenvolvimento.
- `Security/`: geração de tokens, opções JWT, perfis e leitura do usuário autenticado.
- `Exceptions/`: exceções de aplicação e conversão para respostas `ProblemDetails`.
- `Extensions/`: registro de serviços e preparação da aplicação/banco.
- `Program.cs`: composição da API, middleware, autorização, rotas e OpenAPI.
- `backend.http`: exemplos de chamadas que podem ser executados em um cliente HTTP compatível, como a extensão REST Client do VS Code.
- `Services/FuncionarioService.cs`: lista funcionários, fornece opções válidas para cadastro e cria o funcionário com hash de senha.

### Frontend — `frontend/`

- `src/app/pages/`: telas `login`, `ponto`, `espelho`, `ajustes`, `equipe`, `admin` e o layout compartilhado `shell`.
- `src/app/core/api/`: clientes HTTP por área, URL-base (`/api`), tipos TypeScript e utilitários de apresentação.
- `src/app/core/auth/`: estado da autenticação, guards de rota e interceptor de token/renovação.
- `src/app/app.routes.ts`: rotas e perfil necessário em cada área.
- `src/app/app.config.ts`: configuração do roteador, HTTP e interceptor.
- `src/styles.scss` e estilos junto aos componentes: estilos globais e específicos da interface.
- `proxy.conf.json`: proxy do servidor local de desenvolvimento.
- `Dockerfile` e `nginx.conf`: build de produção Angular e publicação com encaminhamento de `/api`.
- `package.json`: comandos de desenvolvimento, build, testes e dependências.

### Raiz do repositório

- `compose.yaml`: serviços de PostgreSQL, API e frontend.
- `.env.example`: nomes e valores de exemplo das variáveis usadas pelo Compose. Copie para `.env` se precisar personalizá-las.

## API HTTP

Os endpoints de negócio exigem autenticação, exceto login, renovação e saída. O corpo e as respostas usam JSON; os enums são serializados como strings. Erros reconhecidos pela aplicação são retornados em `ProblemDetails`.

| Método | Endpoint | Permissão | Descrição |
|---|---|---|---|
| `POST` | `/api/autenticacao/login` | Público | Valida e-mail e senha e emite tokens. |
| `POST` | `/api/autenticacao/renovar` | Público | Rotaciona o token de renovação e emite novo par de tokens. |
| `POST` | `/api/autenticacao/sair` | Público | Revoga o token de renovação informado. |
| `POST` | `/api/autenticacao/alterar-senha-inicial` | Token de primeiro acesso | Define a senha pessoal (mínimo de 12 caracteres) e revoga tokens de renovação anteriores. |
| `GET` | `/api/eu` | Autenticado | Retorna o perfil do usuário atual. |
| `GET` | `/api/eu/status` | Autenticado | Retorna estado, última marcação, minutos da jornada e próximas marcações permitidas. |
| `POST` | `/api/marcacoes` | Autenticado | Registra ponto; retorna `201` ao criar e `200` quando a chave idempotente já existia. |
| `GET` | `/api/marcacoes?de=AAAA-MM-DD&ate=AAAA-MM-DD` | Autenticado | Lista as marcações do usuário no intervalo; datas são opcionais. |
| `GET` | `/api/espelhos/{competencia}` | Autenticado | Retorna o espelho mensal próprio, com `competencia` no formato `AAAA-MM`. |
| `POST` | `/api/ajustes` | Autenticado | Cria uma solicitação de ajuste. |
| `GET` | `/api/ajustes?status=Pendente` | Autenticado | Lista solicitações visíveis ao usuário; filtro de status é opcional. |
| `POST` | `/api/ajustes/{id}/aprovar` | Gestor ou RH | Aprova pedido pendente; comentário é opcional. |
| `POST` | `/api/ajustes/{id}/rejeitar` | Gestor ou RH | Rejeita pedido pendente; comentário é opcional. |
| `GET` | `/api/equipe/status` | Gestor ou RH | Lista o status atual da equipe visível. |
| `GET` | `/api/equipe/{funcionarioId}/espelhos/{competencia}` | Gestor ou RH | Consulta o espelho mensal de um funcionário autorizado. |
| `GET` | `/api/admin/funcionarios` | RH | Lista funcionários. |
| `POST` | `/api/admin/periodos/{competencia}/fechar?codigoPais=BR` | RH | Fecha a competência para o país informado. |
| `GET` | `/api/admin/relatorios/{competencia}.csv?codigoPais=BR` | RH | Baixa relatório CSV; o filtro de país é opcional. |
| `GET` | `/api/admin/funcionarios/opcoes-cadastro` | RH | Lista países, jornadas contratuais e gestores disponíveis quando o RH abre o formulário de cadastro. |
| `POST` | `/api/admin/funcionarios` | RH | Cadastra funcionário e retorna seus dados de perfil (nunca a senha). |

No modo `Development`, o documento OpenAPI fica disponível em `http://localhost:5287/openapi/v1.json`. Exemplos completos de autenticação e chamadas estão em `backend/backend.http`.

## Executar com Docker Compose

Requisitos: Docker Desktop com suporte a Docker Compose.

Na raiz do repositório:

```powershell
Copy-Item .env.example .env
docker compose up --build
```

Copiar `.env.example` é opcional se os padrões locais servirem. Para iniciar sem acompanhar os logs, acrescente `-d`.

| Serviço | Endereço local |
|---|---|
| Frontend (Nginx) | `http://localhost:4200` |
| API | `http://localhost:5287` |
| PostgreSQL | `localhost:5433` |

O banco usa um volume Docker chamado `ponto_postgres_data`. A API aguarda o health check do PostgreSQL e aplica as migrations na inicialização. Em `Development`, também tenta inserir dados de demonstração.

As variáveis de `.env.example` controlam banco, usuário, senha e porta externa do PostgreSQL. Ajustes de configuração da API podem ser feitos pelas variáveis de ambiente padrão do .NET; no Compose, a connection string e a origem CORS já são configuradas para a rede dos serviços.

Este Compose é para desenvolvimento local: ele inicia a API em `Development`, onde podem ser criadas contas de demonstração com senhas conhecidas. Não o exponha como implantação de produção.

## Executar em desenvolvimento local

Requisitos: .NET 10 SDK, Node.js compatível com Angular 21, npm e Docker Desktop (para executar somente o PostgreSQL).

1. Na raiz, inicie o banco:

   ```powershell
   docker compose up -d database
   ```

2. Em outro terminal PowerShell, inicie a API:

   ```powershell
   Set-Location .\backend
   dotnet run
   ```

   As configurações de desenvolvimento do backend conectam ao PostgreSQL em `localhost:5433` e iniciam a API em `http://localhost:5287`. A configuração JWT precisa estar definida; use os valores de desenvolvimento locais apenas para desenvolvimento.

3. Em um terceiro terminal, inicie o frontend:

   ```powershell
   Set-Location .\frontend
   npm install
   npm start
   ```

4. Abra `http://localhost:4200`. As requisições `/api` são encaminhadas pelo proxy do Angular à API local.

Para trocar a porta, credenciais do banco, chave JWT, emissor/audiência JWT ou origens CORS, atualize a configuração correspondente em `backend/appsettings*.json` ou use variáveis de ambiente do ASP.NET Core. Não use senhas ou chaves de desenvolvimento em produção; forneça segredos por um mecanismo próprio do ambiente de implantação.

## Build, testes e migrations

### Frontend

```powershell
Set-Location .\frontend
npm run build
npm test
```

O build padrão usa a configuração de produção do Angular. `npm start` inicia o servidor de desenvolvimento; `npm run watch` compila continuamente em modo development.
Há um arquivo `app.spec.ts`, mas no estado atual o comando `npm test` compila a especificação e termina com `No test files found`; portanto, a suíte frontend ainda não está executando testes. Não há projeto de testes automatizados do backend.

### Backend

```powershell
Set-Location .\backend
dotnet build
```

As migrations são aplicadas automaticamente pela API durante a inicialização. O campo de troca obrigatória da senha inicial é criado pela migration `20261012141626_SenhaInicialObrigatoria`. Para criar uma migration após alterar entidades ou configurações do EF Core, com a ferramenta `dotnet-ef` disponível:

```powershell
Set-Location .\backend
dotnet ef migrations add NomeDescritivo -o Data/Migrations
```

Revise a migration gerada antes de aplicá-la.

## Dados de demonstração

Em ambiente `Development`, o semeador executa após as migrations e insere os dados iniciais somente quando ainda não existe nenhum país no banco. Assim, se o banco já tiver sido inicializado parcialmente ou possuir países, ele não completa automaticamente os demais dados.

| E-mail | Perfil | País |
|---|---|---|
| `rh@dreams.local` | RH | Brasil |
| `gestor@dreams.local` | Gestor | Brasil |
| `colaborador@dreams.local` | Colaborador | Brasil |
| `colaborador.pt@dreams.local` | Colaborador | Portugal |

Senha de todas as contas de demonstração: `Senha@123`. São credenciais exclusivas para desenvolvimento local. O semeador também cria jornadas contratuais e feriados de 2026 para Brasil e Portugal.

## Escopo atual e observações

- A aplicação contém telas e endpoints para registro de ponto, espelho mensal, ajustes, equipe, administração, fechamento e exportação CSV.
- O modelo de dados inclui entidades de feriado e ausência e o espelho considera esses registros. Neste repositório, os feriados de demonstração são semeados para 2026; não há tela ou endpoint dedicado para cadastrar feriados ou ausências.
- O cliente disponível é uma aplicação web Angular. Embora o backend represente origens como `Celular` e `Offline`, não há aplicativo móvel nem fluxo de sincronização offline na interface atual.
- A tela administrativa lista funcionários, fecha períodos e exporta relatórios; cadastrar um funcionário é opcional. Não há fluxo para editar ou desativar um cadastro já criado.
- O cadastro administrativo aceita perfis Colaborador e Gestor; não há criação de contas RH pelo formulário nem redefinição de senha. A senha inicial de novos cadastros precisa ser trocada no primeiro acesso.
- A API não oferece endpoint de reabertura de competência.
- Os dados de demonstração não são apropriados para produção. Configure credenciais, chave JWT, CORS, HTTPS, armazenamento persistente e processo de migração adequados antes de implantar.
