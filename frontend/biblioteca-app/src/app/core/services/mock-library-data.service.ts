import { Injectable, computed, signal } from '@angular/core';
import {
  ActivityItem,
  CurrentUser,
  DashboardStat,
  NavItem,
  QuickAction
} from '../../shared/models/dashboard.model';
import { AccentTone, UserRecord, UserRole, UserRoleCode, UserStatus, UserStatusCode } from '../../shared/models/user.model';

function roleCodeFromLabel(role: UserRole): UserRoleCode {
  switch (role) {
    case 'Administrador':
      return 'ADMIN';
    case 'Bibliotecario':
      return 'LIBRARIAN';
    case 'Profesor':
      return 'TEACHER';
    case 'Estudiante':
      return 'STUDENT';
  }
}

function statusCodeFromLabel(status: UserStatus): UserStatusCode {
  switch (status) {
    case 'Activo':
      return 'active';
    case 'Pendiente':
      return 'pending_verification';
    case 'Suspendido':
      return 'suspended';
    case 'Inactivo':
      return 'inactive';
  }
}

function createMockUser(input: {
  id: string;
  username: string;
  universityId: string;
  firstName: string;
  lastName: string;
  email: string;
  role: UserRole;
  status: UserStatus;
  initials: string;
  accent: AccentTone;
}): UserRecord {
  return {
    id: input.id,
    username: input.username,
    universityId: input.universityId,
    firstName: input.firstName,
    lastName: input.lastName,
    displayName: `${input.firstName} ${input.lastName}`.trim(),
    email: input.email,
    roleCode: roleCodeFromLabel(input.role),
    role: input.role,
    statusCode: statusCodeFromLabel(input.status),
    status: input.status,
    preferredLocale: 'es_MX',
    documentType: 'university_id',
    documentNumber: input.universityId,
    metadataJson: null,
    initials: input.initials,
    accent: input.accent
  };
}

@Injectable({ providedIn: 'root' })
export class MockLibraryDataService {
  readonly currentUser = signal<CurrentUser>({
    name: 'Carlos Garcia Mendoza',
    role: 'Administrador',
    email: 'c.garcia@universidad.edu',
    initials: 'CG',
    code: 'ADM-2024-001'
  });

  readonly navItems: NavItem[] = [
    { label: 'Dashboard', route: '/dashboard', icon: 'dashboard' },
    { label: 'Usuarios', route: '/usuarios', icon: 'users' },
    { label: 'Buscar Libros', route: '/dashboard/catalogo', icon: 'search' },
    { label: 'Gestion Libros', route: '/dashboard/libros', icon: 'library' },
    { label: 'Mi Perfil', route: '/dashboard/perfil', icon: 'profile' }
  ];

  readonly roleOptions: ReadonlyArray<'Todos los roles' | UserRole> = [
    'Todos los roles',
    'Administrador',
    'Bibliotecario',
    'Profesor',
    'Estudiante'
  ];

  readonly users = signal<UserRecord[]>([
    createMockUser({
      id: 'usr-001',
      username: 'cgarcia',
      universityId: 'ADM-2024-001',
      firstName: 'Carlos',
      lastName: 'Garcia Mendoza',
      email: 'c.garcia@universidad.edu',
      role: 'Administrador',
      status: 'Activo',
      initials: 'CG',
      accent: 'blue'
    }),
    createMockUser({
      id: 'usr-002',
      username: 'mlopez',
      universityId: 'PRF-2024-042',
      firstName: 'Maria',
      lastName: 'Lopez Torres',
      email: 'm.lopez@universidad.edu',
      role: 'Profesor',
      status: 'Activo',
      initials: 'ML',
      accent: 'violet'
    }),
    createMockUser({
      id: 'usr-003',
      username: 'lramirez',
      universityId: 'EST-2024-198',
      firstName: 'Luis',
      lastName: 'Ramirez Perez',
      email: 'l.ramirez@universidad.edu',
      role: 'Estudiante',
      status: 'Activo',
      initials: 'LR',
      accent: 'green'
    }),
    createMockUser({
      id: 'usr-004',
      username: 'amartinez',
      universityId: 'EST-2024-219',
      firstName: 'Ana Sofia',
      lastName: 'Martinez Ruiz',
      email: 'a.martinez@universidad.edu',
      role: 'Estudiante',
      status: 'Activo',
      initials: 'AM',
      accent: 'green'
    }),
    createMockUser({
      id: 'usr-005',
      username: 'rherrera',
      universityId: 'PRF-2024-017',
      firstName: 'Roberto',
      lastName: 'Herrera Salinas',
      email: 'r.herrera@universidad.edu',
      role: 'Profesor',
      status: 'Pendiente',
      initials: 'RH',
      accent: 'amber'
    }),
    createMockUser({
      id: 'usr-006',
      username: 'dvega',
      universityId: 'EST-2024-277',
      firstName: 'Daniela',
      lastName: 'Vega Ponce',
      email: 'd.vega@universidad.edu',
      role: 'Estudiante',
      status: 'Activo',
      initials: 'DV',
      accent: 'slate'
    }),
    createMockUser({
      id: 'usr-007',
      username: 'fsanchez',
      universityId: 'ADM-2024-008',
      firstName: 'Fernanda',
      lastName: 'Sanchez Ibarra',
      email: 'f.sanchez@universidad.edu',
      role: 'Administrador',
      status: 'Activo',
      initials: 'FS',
      accent: 'blue'
    }),
    createMockUser({
      id: 'usr-008',
      username: 'jcastillo',
      universityId: 'EST-2024-301',
      firstName: 'Jorge',
      lastName: 'Castillo Nava',
      email: 'j.castillo@universidad.edu',
      role: 'Estudiante',
      status: 'Suspendido',
      initials: 'JC',
      accent: 'amber'
    })
  ]);

  readonly dashboardStats = signal<DashboardStat[]>([
    {
      label: 'Total Libros',
      value: '68',
      note: '+3 este mes',
      detail: 'Catalogo actualizado',
      tone: 'blue',
      icon: 'books'
    },
    {
      label: 'Total Usuarios',
      value: '542',
      note: '+12 este mes',
      detail: 'Comunidad activa',
      tone: 'violet',
      icon: 'users'
    },
    {
      label: 'Libros Disponibles',
      value: '41',
      note: '60% del total',
      detail: 'Disponibilidad actual',
      tone: 'green',
      icon: 'available'
    },
    {
      label: 'Libros Prestados',
      value: '27',
      note: '40% del total',
      detail: 'Prestamos en curso',
      tone: 'amber',
      icon: 'loans'
    }
  ]);

  readonly quickActions = signal<QuickAction[]>([
    {
      title: 'Gestionar Usuarios',
      description: 'Altas, roles y estados de cuentas institucionales.',
      route: '/usuarios',
      tone: 'blue',
      icon: 'users'
    },
    {
      title: 'Gestionar Libros',
      description: 'Alta de catalogo y ficha bibliografica.',
      route: '/dashboard',
      tone: 'violet',
      icon: 'books'
    },
    {
      title: 'Buscar Recursos',
      description: 'Exploracion rapida de catalogo y disponibilidad.',
      route: '/dashboard',
      tone: 'green',
      icon: 'search'
    }
  ]);

  readonly recentActivity = signal<ActivityItem[]>([
    {
      id: 'act-001',
      title: 'Luis Ramirez reservó "Calculo Diferencial e Integral"',
      subtitle: 'Reserva de libro desde portal estudiantil',
      timeLabel: 'Hace 5 min',
      tone: 'blue',
      icon: 'book'
    },
    {
      id: 'act-002',
      title: 'Nuevo usuario registrado: Ana Sofia Martinez',
      subtitle: 'Alta aprobada por servicios escolares',
      timeLabel: 'Hace 20 min',
      tone: 'violet',
      icon: 'user'
    },
    {
      id: 'act-003',
      title: '"Fisica Universitaria" devuelto por Dr. Roberto Herrera',
      subtitle: 'Prestamo cerrado en mostrador',
      timeLabel: 'Hace 1 hora',
      tone: 'green',
      icon: 'return'
    },
    {
      id: 'act-004',
      title: 'Quedan 2 ejemplares de "Introduccion a Bases de Datos"',
      subtitle: 'Revisar reposicion o restriccion temporal',
      timeLabel: 'Hace 2 horas',
      tone: 'amber',
      icon: 'alert'
    }
  ]);

  private readonly dashboardClock = signal(new Date('2026-03-22T23:35:00'));

  readonly dashboardDateLabel = computed(() =>
    new Intl.DateTimeFormat('es-MX', {
      weekday: 'long',
      day: 'numeric',
      month: 'long',
      year: 'numeric'
    }).format(this.dashboardClock()));

  readonly dashboardTimeLabel = computed(() =>
    new Intl.DateTimeFormat('es-MX', {
      hour: 'numeric',
      minute: '2-digit',
      hour12: true
    }).format(this.dashboardClock()));
}
