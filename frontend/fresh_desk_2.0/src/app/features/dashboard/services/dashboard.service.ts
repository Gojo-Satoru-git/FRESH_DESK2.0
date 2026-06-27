import { Observable } from 'rxjs';

export interface DashboardPerformance {
  resolvedToday: number;
  receivedToday: number;
  resolutionRate: number | null;
}

export interface DashboardGroupMetric {
  groupName: string;
  ticketCount: number;
}

export interface DashboardTodo {
  id: string;
  title: string;
  due: 'critical' | 'high' | 'normal' | 'None';
}

export interface DashboardTrend {
  timeLabel: string;
  todayCount: number;
  yesterdayCount: number;
}

export interface DashboardData {
  totalTickets: number;
  totalActive: number;
  inProgress: number;
  pendingReply: number;
  resolvedClosed: number;
  counts: { unassigned: number };
  performance: DashboardPerformance;
  groupMetrics: DashboardGroupMetric[];
  todos: DashboardTodo[];
  trends: DashboardTrend[];
}

export abstract class DashboardService {
  abstract getDashboard(): Observable<DashboardData>;
}