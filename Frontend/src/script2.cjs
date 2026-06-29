const fs = require('fs');
const file = 'c:/Users/tua46/OneDrive/Máy tính/DATN/QuanLyCongViec/Frontend/src/views/HomeSite/Goals/GoalsDashboard.vue';
let content = fs.readFileSync(file, 'utf8');

const newCSS = `
:deep(.jira-date-picker .el-input__inner) {
  border: none !important;
  padding: 0 !important;
  height: auto !important;
}
`;

content = content.replace('</style>', newCSS + '</style>');
fs.writeFileSync(file, content, 'utf8');
console.log('Success');
