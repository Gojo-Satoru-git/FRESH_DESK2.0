export const PERMISSIONS = {
  DASHBOARD: {
    VIEW_ADMIN: 'dashboard:view_admin',
    VIEW_MANAGER: 'dashboard:view_manager',
    VIEW_TEAM_LEAD: 'dashboard:view_team_lead',
    VIEW_AGENT: 'dashboard:view_agent',
    VIEW_CUSTOMER: 'dashboard:view_customer',
  },
  TICKET: {
    READ: 'ticket:read',
    READ_OWN: 'ticket:read_own',
    READ_ASSIGNED: 'ticket:read_assigned',
    READ_TEAM: 'ticket:read_team',
    READ_QUEUE: 'ticket:read_queue',
    READ_ALL: 'ticket:read_all',
    READ_COMPANY: 'ticket:read_company',
    CREATE: 'ticket:create',
    UPDATE: 'ticket:update',
    DELETE: 'ticket:delete',
    CLOSE: 'ticket:close',
    REOPEN: 'ticket:reopen',
    RESOLVE: 'ticket:resolve',
    REASSIGN: 'ticket:reassign',
    ASSIGN: 'ticket:assign',
    BULK_ASSIGN: 'ticket:bulk_assign',
    BULK_UPDATE: 'ticket:bulk_update',
    COMMENT: 'ticket:comment',
    ATTACHMENT_UPLOAD: 'ticket:attachment_upload',
    CHANGE_PRIORITY: 'ticket:change_priority',
  },
  KB: {
    READ: 'kb:read',
    CREATE: 'kb:create',
    UPDATE: 'kb:update',
    DELETE: 'kb:delete',
    PUBLISH: 'kb:publish',
    ARCHIVE: 'kb:archive',
    MANAGE_FOLDERS: 'kb:manage_folders',
  },
} as const;

export type Permission =
  (typeof PERMISSIONS)[keyof typeof PERMISSIONS][keyof (typeof PERMISSIONS)[keyof typeof PERMISSIONS]];
