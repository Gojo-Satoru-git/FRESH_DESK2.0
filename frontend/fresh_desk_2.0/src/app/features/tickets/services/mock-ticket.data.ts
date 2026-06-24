import {
  TicketActivity,
  TicketComment,
  TicketDetails,
  TicketListItem,
} from '../models/ticket.model';

export interface MockAgent {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
}

export const mockAgents: MockAgent[] = [
  {
    id: 'agent-001',
    firstName: 'Anika',
    lastName: 'Rao',
    email: 'anika.rao@example.com',
  },
  {
    id: 'agent-002',
    firstName: 'Dev',
    lastName: 'Menon',
    email: 'dev.menon@example.com',
  },
  {
    id: 'agent-003',
    firstName: 'Sara',
    lastName: 'Khan',
    email: 'sara.khan@example.com',
  },
];

export const mockComments: Record<string, TicketComment[]> = {
  'ticket-001': [
    {
      id: 'comment-001',
      authorId: 'agent-001',
      body: 'I reproduced this in the staging tenant and asked the integration team to verify the webhook payload.',
      visibility: 'Internal',
      createdAt: '2026-06-22T09:20:00Z',
      attachments: [],
      authorName: 'Anika Rao',
    },
  ],
  'ticket-002': [
    {
      id: 'comment-002',
      contactId: 'contact-002',
      body: 'The issue is still happening for two users in the payroll team.',
      visibility: 'Public',
      createdAt: '2026-06-23T14:15:00Z',
      attachments: [],
      contactName: 'Priya Nair',
    },
  ],
};

export const mockActivities: Record<string, TicketActivity[]> = {
  'ticket-001': [
    {
      id: 'activity-001',
      activityType: 'StatusChanged',
      oldValue: 'Open',
      newValue: 'InProgress',
      performedBy: 'agent-001',
      performedAt: '2026-06-22T09:10:00Z',
      performedByName: 'Anika Rao',
    },
  ],
  'ticket-002': [
    {
      id: 'activity-002',
      activityType: 'Assigned',
      newValue: 'Dev Menon',
      performedBy: 'agent-002',
      performedAt: '2026-06-23T13:55:00Z',
      performedByName: 'Dev Menon',
    },
  ],
};

export const mockTickets: TicketDetails[] = [
  {
    id: 'ticket-001',
    ticketNumber: 'FD-1001',
    title: 'Webhook delivery fails for payroll approvals',
    description:
      'Payroll approval webhooks are timing out after the recent tenant configuration update.',
    status: 'InProgress',
    priority: 'High',
    category: 'Integration',
    assignedAgentId: 'agent-001',
    reporterId: 'contact-001',
    companyId: 'company-001',
    department: 'Payroll',
    region: 'India',
    moduleName: 'Workflow',
    createdAt: '2026-06-21T08:30:00Z',
    updatedAt: '2026-06-23T10:45:00Z',
    tags: ['webhook', 'payroll', 'approval'],
    type: 'Bug',
    reporterName: 'Rohan Iyer',
    assignedAgentName: 'Anika Rao',
    comments: mockComments['ticket-001'],
    statusHistory: [
      {
        id: 'history-001',
        oldStatus: 'Open',
        newStatus: 'InProgress',
        changedBy: 'agent-001',
        changedAt: '2026-06-22T09:10:00Z',
        reason: 'Reproduced locally',
      },
    ],
    assignmentLogs: [
      {
        id: 'assignment-001',
        agentId: 'agent-001',
        assignedBy: 'lead-001',
        assignedAt: '2026-06-21T09:00:00Z',
      },
    ],
    activities: mockActivities['ticket-001'],
    attachments: [],
  },
  {
    id: 'ticket-002',
    ticketNumber: 'FD-1002',
    title: 'Payslip PDF preview shows blank page',
    description:
      'Customer reports that payslip previews open successfully but render as a blank page in the browser.',
    status: 'Pending',
    priority: 'Medium',
    category: 'Document',
    assignedAgentId: 'agent-002',
    reporterId: 'contact-002',
    companyId: 'company-002',
    department: 'HR Operations',
    region: 'UAE',
    moduleName: 'Payroll',
    createdAt: '2026-06-23T12:40:00Z',
    updatedAt: '2026-06-23T14:20:00Z',
    tags: ['pdf', 'preview'],
    type: 'Support',
    reporterName: 'Priya Nair',
    assignedAgentName: 'Dev Menon',
    comments: mockComments['ticket-002'],
    statusHistory: [],
    assignmentLogs: [
      {
        id: 'assignment-002',
        agentId: 'agent-002',
        assignedBy: 'lead-001',
        assignedAt: '2026-06-23T13:55:00Z',
      },
    ],
    activities: mockActivities['ticket-002'],
    attachments: [],
  },
  {
    id: 'ticket-003',
    ticketNumber: 'FD-1003',
    title: 'New field request for employee onboarding',
    description: 'Add a mandatory cost center field to the onboarding form for finance reporting.',
    status: 'Open',
    priority: 'Low',
    category: 'FeatureRequest',
    companyId: 'company-001',
    department: 'People Ops',
    region: 'India',
    moduleName: 'Onboarding',
    createdAt: '2026-06-24T06:05:00Z',
    tags: ['onboarding', 'form'],
    type: 'FeatureRequest',
    reporterName: 'Meera Das',
    comments: [],
    statusHistory: [],
    assignmentLogs: [],
    activities: [],
    attachments: [],
  },
];

export function toTicketListItem(ticket: TicketDetails): TicketListItem {
  return {
    id: ticket.id,
    ticketNumber: ticket.ticketNumber ?? ticket.id,
    title: ticket.title,
    status: ticket.status,
    priority: ticket.priority,
    descriptionPreview: ticket.description.slice(0, 120),
    assignedAgentId: ticket.assignedAgentId,
    companyId: ticket.companyId,
    createdAt: ticket.createdAt,
    updatedAt: ticket.updatedAt,
  };
}
