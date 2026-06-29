const fs = require('fs');

const path = 'c:\\Users\\tua46\\OneDrive\\Máy tính\\DATN\\QuanLyCongViec\\Frontend\\src\\views\\HomeSite\\Teams\\TeamsDashboard.vue';
let content = fs.readFileSync(path, 'utf8');

const old_filtered_users = `const filteredUsers = computed(() => {
  const allUsers = peopleStore.users.map(u => ({
    id: u.id,
    fullName: u.fullName || u.email,
    initials: u.initials || getInitials(u.fullName, u.email),
    avatarColor: u.avatarColor || getAvatarColor(u.id || u.email),
    avatarUrl: u.avatarUrl
  }))`;

const new_filtered_users = `const filteredUsers = computed(() => {
  const allUsers = peopleStore.users.map(u => ({
    id: u.id,
    fullName: u.fullName || u.email,
    email: u.email,
    initials: getInitials(u.fullName, u.email),
    avatarColor: getAvatarColor(u.email || u.id),
    avatarUrl: u.avatarUrl
  }))`;

content = content.replace(old_filtered_users, new_filtered_users);

// Also fix currentMemberObj in openCreateTeamModal
const old_current_member = `  const currentMemberObj = storeUser ? {
    id: storeUser.id,
    fullName: storeUser.fullName || storeUser.name || sessionUser.fullName || sessionUser.email,
    initials: storeUser.initials || sessionUser.initials,
    avatarColor: storeUser.avatarColor || sessionUser.avatarColor || getAvatarColor(storeUser.email || sessionUser.email || storeUser.id || sessionUser.id),
    avatarUrl: storeUser.avatarUrl || sessionUser.avatarUrl
  } : null;`;

const new_current_member = `  const currentMemberObj = storeUser ? {
    id: storeUser.id,
    fullName: storeUser.fullName || storeUser.name || sessionUser.fullName || sessionUser.email,
    email: storeUser.email || sessionUser.email,
    initials: getInitials(storeUser.fullName || sessionUser.fullName, storeUser.email || sessionUser.email),
    avatarColor: getAvatarColor(storeUser.email || sessionUser.email || storeUser.id || sessionUser.id),
    avatarUrl: storeUser.avatarUrl || sessionUser.avatarUrl
  } : null;`;

content = content.replace(old_current_member, new_current_member);

fs.writeFileSync(path, content, 'utf8');
console.log("TeamsDashboard updated");
