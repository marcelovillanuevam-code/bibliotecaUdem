export interface PorMes {
  mes: string;
  total: number;
  conteo: number;
}

export interface PorMotivo {
  motivo: string;
  total: number;
  conteo: number;
}

export interface MultasRecaudadas {
  totalGeneral: number;
  totalConteo: number;
  porMes: PorMes[];
  porMotivo: PorMotivo[];
}

export interface Deudor {
  userId: string;
  nombreUsuario: string;
  emailUsuario: string;
  cantidadMultas: number;
  totalPendiente: number;
}

export interface DevolucionesTardias {
  totalDevoluciones: number;
  totalTardias: number;
  porcentajeTardias: number;
  promedioDiasRetraso: number | null;
}

export interface Condonacion {
  id: string;
  deudorId: string;
  nombreDeudor: string;
  emailDeudor: string;
  motivo: string;
  motivoCondonacion: string | null;
  monto: number;
  condonadaEn: string;
  condonadaPorId: string | null;
  nombreCondonador: string;
}

export interface ReporteFilters {
  from: string;
  to: string;
}
