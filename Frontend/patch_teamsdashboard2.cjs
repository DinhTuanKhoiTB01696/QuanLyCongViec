const fs = require('fs');

const path = 'c:\\Users\\tua46\\OneDrive\\Máy tính\\DATN\\QuanLyCongViec\\Frontend\\src\\views\\HomeSite\\Teams\\TeamsDashboard.vue';
let content = fs.readFileSync(path, 'utf8');

const target1 = `<UserAvatar :user="member" :size="20" :fontSize="10" />
                 {{ member.fullName }}`;
const repl1 = `<UserAvatar :user="member" :size="20" :fontSize="10" />
                 {{ member.fullName || member.name || member.email }}`;

content = content.replace(target1, repl1);

const target2 = `<UserAvatar :user="user" :size="20" :fontSize="10" />
                <span>{{ user.fullName }}</span>`;
const repl2 = `<UserAvatar :user="user" :size="20" :fontSize="10" />
                <span>{{ user.fullName || user.name || user.email }}</span>`;

content = content.replace(target2, repl2);

fs.writeFileSync(path, content, 'utf8');
console.log("TeamsDashboard template patched");
