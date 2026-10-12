import { HttpErrorResponse } from '@angular/common/http';
import { ProblemDetails } from './models';


export function fusoDoNavegador(): string {
  return Intl.DateTimeFormat().resolvedOptions().timeZone;
}


export function competenciaDe(data: Date = new Date()): string {
  const mes = String(data.getMonth() + 1).padStart(2, '0');
  return `${data.getFullYear()}-${mes}`;
}


export function dataIso(data: Date): string {
  const mes = String(data.getMonth() + 1).padStart(2, '0');
  const dia = String(data.getDate()).padStart(2, '0');
  return `${data.getFullYear()}-${mes}-${dia}`;
}


export function mensagemDeErro(erro: unknown): string {
  if (!(erro instanceof HttpErrorResponse)) {
    return 'Ocorreu um erro inesperado.';
  }

  if (erro.status === 0) {
    return 'Não foi possível conectar à API. Verifique se o backend está rodando.';
  }

  const problema = erro.error as ProblemDetails | null;

  if (problema?.errors) {
    const mensagens = Object.values(problema.errors).flat();
    if (mensagens.length > 0) {
      return mensagens.join(' ');
    }
  }

  return problema?.detail ?? problema?.title ?? `Erro ${erro.status}.`;
}


export function baixarArquivo(conteudo: Blob, nomeArquivo: string): void {
  const url = URL.createObjectURL(conteudo);
  const link = document.createElement('a');
  link.href = url;
  link.download = nomeArquivo;
  link.click();
  URL.revokeObjectURL(url);
}


export function formatarMinutos(minutos: number): string {
  const sinal = minutos < 0 ? '-' : '';
  const abs = Math.abs(minutos);
  return `${sinal}${Math.floor(abs / 60)}h${String(abs % 60).padStart(2, '0')}`;
}


export function horaDe(iso: string): string {
  return iso.substring(11, 16);
}
