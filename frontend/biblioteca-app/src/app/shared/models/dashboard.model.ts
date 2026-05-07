import { AccentTone } from './user.model';

export type DashboardIcon =
  | 'books'
  | 'copies'
  | 'users'
  | 'available'
  | 'loans'
  | 'fines'
  | 'reservations'
  | 'search';
export type ActivityIcon = 'book' | 'user' | 'return' | 'alert';
export type NavIcon =
  | 'dashboard'
  | 'users'
  | 'search'
  | 'library'
  | 'loans'
  | 'reservations'
  | 'profile'
  | 'returns'
  | 'fines'
  | 'config'
  | 'reports';

export interface DashboardStat {
  label: string;
  value: string;
  note: string;
  detail: string;
  tone: AccentTone;
  icon: DashboardIcon;
}

export interface ActivityItem {
  id: string;
  title: string;
  subtitle: string;
  timeLabel: string;
  tone: AccentTone;
  icon: ActivityIcon;
}

export interface QuickAction {
  title: string;
  description: string;
  route: string;
  tone: AccentTone;
  icon: DashboardIcon;
}

export interface NavItem {
  label: string;
  route: string;
  icon: NavIcon;
}

export interface CurrentUser {
  name: string;
  role: string;
  email: string;
  initials: string;
  code: string;
}

export interface DashboardKpis {
  books: BooksKpis;
  copies: CopiesKpis;
  users: UsersKpis;
  loans: LoansKpis;
  fines: FinesKpis;
  reservations: ReservationsKpis;
  recentActivity: RecentActivity[];
}

export interface BooksKpis {
  total: number;
  active: number;
}

export interface CopiesKpis {
  total: number;
  available: number;
  loaned: number;
  maintenance: number;
}

export interface UsersKpis {
  total: number;
  active: number;
}

export interface LoansKpis {
  active: number;
  overdue: number;
  totalThisMonth: number;
  last30Days: LoanDailyKpi[];
}

export interface LoanDailyKpi {
  date: string;
  total: number;
}

export interface FinesKpis {
  pending: number;
  totalAmountPendingMxn: number;
  paidThisMonth: number;
}

export interface ReservationsKpis {
  active: number;
  ready: number;
}

export interface RecentActivity {
  id: string;
  tableName: string;
  action: 'INSERT' | 'UPDATE' | 'DELETE' | string;
  recordId: string | null;
  performedBy: string | null;
  performedAt: string;
  summary: string;
}
