import { Injectable, inject } from '@angular/core';
import { Observable, of, delay } from 'rxjs';
import { DashboardService, DashboardData } from './dashboard.service';
import { AuthService } from '../../../core/auth/auth.service';

@Injectable()
export class MockDashboardService extends DashboardService {
  private authService = inject(AuthService);

  override getDashboard(): Observable<DashboardData> {
    const user = this.authService.currentUser();
    const role = user?.role || 'agent';

    let mockData: DashboardData;

    switch (role) {
      case 'admin':
        // ==========================================
        // ADMIN VIEW: Global, organization-wide data (Matches your screenshot)
        // ==========================================
        mockData = {
          totalTickets: 142,
          totalActive: 45,
          inProgress: 18,
          pendingReply: 5,
          resolvedClosed: 97,
          counts: { unassigned: 14 },
          performance: { resolvedToday: 24, receivedToday: 28, resolutionRate: 85 },
          groupMetrics: [
            { groupName: 'Network Operations', ticketCount: 12 },
            { groupName: 'Database Admin', ticketCount: 4 },
            { groupName: 'EMEA Support', ticketCount: 29 }
          ],
          todos: [
            { id: '1', title: 'Review major incident report (Global)', due: 'critical' },
            { id: '2', title: 'Approve software requests', due: 'high' }
          ],
          trends: [
            { timeLabel: '08:00 AM', todayCount: 5, yesterdayCount: 4 },
            { timeLabel: '10:00 AM', todayCount: 15, yesterdayCount: 12 },
            { timeLabel: '12:00 PM', todayCount: 28, yesterdayCount: 22 },
            { timeLabel: '02:00 PM', todayCount: 35, yesterdayCount: 40 },
            { timeLabel: '04:00 PM', todayCount: 42, yesterdayCount: 38 },
            { timeLabel: '06:00 PM', todayCount: 45, yesterdayCount: 45 }
          ]
        };
        break;

      case 'team_lead':
        // ==========================================
        // LEAD VIEW: Only tickets in their specific queues
        // ==========================================
        mockData = {
          totalTickets: 56,
          totalActive: 22,
          inProgress: 10,
          pendingReply: 3,
          resolvedClosed: 34,
          counts: { unassigned: 8 },
          performance: { resolvedToday: 12, receivedToday: 15, resolutionRate: 80 },
          groupMetrics: [
            { groupName: 'Network Operations', ticketCount: 12 },
            { groupName: 'EMEA Support', ticketCount: 10 }
          ],
          todos: [
            { id: '1', title: 'Re-assign stale tickets in EMEA', due: 'high' },
            { id: '2', title: 'Approve agent timesheets', due: 'normal' }
          ],
          trends: [
            { timeLabel: '08:00 AM', todayCount: 2, yesterdayCount: 1 },
            { timeLabel: '10:00 AM', todayCount: 6, yesterdayCount: 5 },
            { timeLabel: '12:00 PM', todayCount: 12, yesterdayCount: 10 },
            { timeLabel: '02:00 PM', todayCount: 18, yesterdayCount: 15 },
            { timeLabel: '04:00 PM', todayCount: 20, yesterdayCount: 22 },
            { timeLabel: '06:00 PM', todayCount: 22, yesterdayCount: 24 }
          ]
        };
        break;

      default:
      case 'agent':
        // ==========================================
        // AGENT VIEW: Strictly tickets assigned directly to them
        // ==========================================
        mockData = {
          totalTickets: 18,
          totalActive: 6,
          inProgress: 4,
          pendingReply: 2,
          resolvedClosed: 12,
          counts: { unassigned: 0 }, // Agents don't have personal unassigned tickets
          performance: { resolvedToday: 3, receivedToday: 4, resolutionRate: 75 },
          groupMetrics: [
            { groupName: 'My Assigned Work', ticketCount: 6 }
          ],
          todos: [
            { id: '1', title: 'Call back user regarding VPN issue', due: 'high' },
            { id: '2', title: 'Update documentation for Ticket #1042', due: 'normal' }
          ],
          trends: [
            { timeLabel: '08:00 AM', todayCount: 0, yesterdayCount: 0 },
            { timeLabel: '10:00 AM', todayCount: 1, yesterdayCount: 2 },
            { timeLabel: '12:00 PM', todayCount: 3, yesterdayCount: 2 },
            { timeLabel: '02:00 PM', todayCount: 4, yesterdayCount: 5 },
            { timeLabel: '04:00 PM', todayCount: 5, yesterdayCount: 5 },
            { timeLabel: '06:00 PM', todayCount: 6, yesterdayCount: 6 }
          ]
        };
        break;
    }

    // Return the specific payload with a simulated network delay
    return of(mockData).pipe(delay(500));
  }
}