const fs = require('fs');
let c = fs.readFileSync('src/views/HomeSite/Teams/TeamsDashboard.vue', 'utf8');
const toggleCss = `
/* View Toggle */
.view-toggle {
  display: flex;
  border: 1px solid #DFE1E6;
  border-radius: 3px;
  overflow: hidden;
  height: 32px;
}

.toggle-btn {
  background: #FAFBFC;
  border: none;
  padding: 0 12px;
  color: #5E6C84;
  cursor: pointer;
  font-size: 14px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background-color 0.2s, color 0.2s;
}

.toggle-btn:hover {
  background-color: #EBECF0;
}

.toggle-btn.active {
  background-color: #DEEBFF;
  color: #0052CC;
}`;

if (!c.includes('.toggle-btn.active')) {
  c = c.replace('</style>', toggleCss + '\n</style>');
}

c = c.replace(/<div class="view-toggle" style="[^"]*">/, '<div class="view-toggle">');
c = c.replace(/<button class="icon-btn toggle-btn" \:class/g, '<button class="toggle-btn" :class');

fs.writeFileSync('src/views/HomeSite/Teams/TeamsDashboard.vue', c);
console.log('Fixed CSS');
