import { AccentTone } from './user.model';

export type DashboardIcon = 'books' | 'users' | 'available' | 'loans' | 'search';
export type ActivityIcon = 'book' | 'user' | 'return' | 'alert';
export type NavIcon = 'dashboard' | 'users' | 'search' | 'library' | 'profile';

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
