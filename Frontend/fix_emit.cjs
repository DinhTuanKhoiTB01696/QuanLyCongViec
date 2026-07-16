const fs = require('fs');
const file = 'src/components/TaskDetailModal.vue';
let content = fs.readFileSync(file, 'utf8');

const oldCode = `         await fetchContingencyPlans();\n         fetchAuditTimeline();`;

const newCode = `         await fetchContingencyPlans();\n         fetchAuditTimeline();\n         emit('updated');`;

content = content.replace(oldCode, newCode);
if (!content.includes(newCode)) {
    // try with \r\n
    const oldCode2 = `         await fetchContingencyPlans();\r\n         fetchAuditTimeline();`;
    const newCode2 = `         await fetchContingencyPlans();\r\n         fetchAuditTimeline();\r\n         emit('updated');`;
    content = content.replace(oldCode2, newCode2);
}
fs.writeFileSync(file, content, 'utf8');
