const fs = require('fs');
let content = fs.readFileSync('Frontend/src/views/HomeSite/Teams/TeamList.vue', 'utf8');

// The incorrect part:
// avatarText: t.name ? t.name.substring(0,
// memberCount: t.members?.length || t.users?.length || 0,
// ...
// type: t.type || 'Đội ngũ chính thức', 2).toUpperCase() : 'T',

content = content.replace(/avatarText: t\.name \? t\.name\.substring\(0,[\s\S]*?\}\)\)/, `avatarText: t.name ? t.name.substring(0, 2).toUpperCase() : 'T',
    memberCount: t.members?.length || t.users?.length || 0,
    childrenCount: t.children?.length || t.subDepartments?.length || 0,
    manager: t.manager || t.managerId,
    managerName: t.manager?.fullName || t.manager?.name || 'Chưa chọn người quản lý',
    managerEmail: t.manager?.email || '',
    parentTeamName: t.parentDepartment?.name || t.parent?.name || 'Không có đội ngũ gốc',
    type: t.type || 'Đội ngũ chính thức'
  }))`);

fs.writeFileSync('Frontend/src/views/HomeSite/Teams/TeamList.vue', content);
console.log('Fixed TeamList');
