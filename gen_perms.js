const fs = require('fs');
const modules = {
    'admin.roles': ['view', 'create', 'edit', 'delete', 'manage_permissions'],
    'admin.users': ['view', 'invite', 'edit', 'suspend', 'delete'],
    'admin.security': ['view', 'manage_2fa', 'manage_ip_whitelist'],
    'admin.settings': ['view', 'edit'],
    'dashboard': ['view'],
    'profile': ['view', 'edit'],
    'your_work': ['view'],
    'stickies': ['view', 'create', 'edit', 'delete'],
    'rewards': ['view', 'manage'],
    'drafts': ['view', 'create', 'edit', 'delete'],
    'views': ['view', 'create', 'edit', 'delete'],
    'analytics': ['view', 'export'],
    'archives': ['view', 'restore', 'delete'],
    'priority': ['view', 'manage'],
    'chat': ['view', 'send', 'edit', 'delete', 'manage_channel'],
    'feed': ['view', 'post', 'comment', 'like'],
    'checkin': ['view', 'submit', 'review'],
    'integrations': ['view', 'manage'],
    'teams': ['view', 'create', 'edit', 'delete', 'manage_members'],
    'goals': ['view', 'create', 'edit', 'delete', 'update_progress', 'manage_metrics'],
    'projects': ['view', 'create', 'edit', 'delete', 'archive', 'restore', 'export', 'import', 'manage_settings'],
    'people': ['view', 'manage'],
    'audit_log': ['view', 'export'],
    'starred': ['view', 'manage'],
    'notifications': ['view', 'manage'],
    'system_status': ['view'],
    'spaces': ['view', 'create', 'edit', 'delete', 'archive', 'restore', 'manage_settings'],
    'space.work_items': ['view', 'create', 'edit', 'delete', 'assign', 'change_status', 'comment', 'attachment'],
    'space.timeline': ['view', 'edit'],
    'space.cycles': ['view', 'create', 'edit', 'delete'],
    'space.intakes': ['view', 'manage'],
    'space.modules': ['view', 'manage'],
    'space.views': ['view', 'create', 'edit', 'delete'],
    'space.pages': ['view', 'create', 'edit', 'delete'],
    'space.reports': ['view', 'create', 'export'],
    'space.dashboard': ['view', 'edit'],
    'space.members': ['view', 'invite', 'remove', 'manage_roles'],
    'space.settings': ['view', 'edit'],
    'ai_assistant': ['view', 'use']
};

let output = '';
for (const [mod, actions] of Object.entries(modules).sort()) {
    for (const action of actions) {
        let desc = action.replace(/_/g, ' ') + ' ' + mod.replace(/\./g, ' ');
        desc = desc.charAt(0).toUpperCase() + desc.slice(1);
        output += `                new { Module = "${mod}", Code = "${mod}.${action}", Description = "${desc}" },\n`;
    }
    output += '\n';
}
fs.writeFileSync('generated_permissions.txt', output);
