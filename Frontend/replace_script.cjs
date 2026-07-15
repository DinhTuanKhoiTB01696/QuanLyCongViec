const fs = require('fs');
const file = 'c:/Users/tua46/OneDrive/Máy tính/DATN_2/QuanLyCongViec/QuanLyCongViec/Frontend/src/components/TaskDetailModal.vue';
let content = fs.readFileSync(file, 'utf8');
const newLogic = fs.readFileSync('new_logic.txt', 'utf8');

const startStr = 'async function fetchContingencyPlans() {';
const endStr = '// Comments logic';
const startIdx = content.indexOf(startStr);
const endIdx = content.indexOf(endStr);
if (startIdx !== -1 && endIdx !== -1) {
    content = content.substring(0, startIdx) + newLogic + '\n\n' + content.substring(endIdx);
    fs.writeFileSync(file, content, 'utf8');
    console.log('Successfully replaced logic');
} else {
    console.log('Could not find start or end index', startIdx, endIdx);
}
