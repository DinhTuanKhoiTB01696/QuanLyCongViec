const fs = require('fs');
const spaceFile = 'd:\\A\\QuanLyCongViec\\Frontend\\src\\views\\SpaceSummary.vue';
let content = fs.readFileSync(spaceFile, 'utf8');

// Find the start of the first openCreateTask
const startStr = 'const openCreateTask = (statusText) => {';
const startIdx = content.indexOf(startStr);

if (startIdx > -1) {
    // Find the end: after 'handleDraggableChange' block
    const endStr = 'const handleDraggableChange = async (evt, targetGroup) => {';
    const endBlockStart = content.indexOf(endStr, startIdx);
    if (endBlockStart > -1) {
        // Find the next function or comment to know where it ends
        const afterEndStr = '// SignalR event handlers';
        const afterEndIdx = content.indexOf(afterEndStr, endBlockStart);
        if (afterEndIdx > -1) {
            let newContent = content.substring(0, startIdx) + content.substring(afterEndIdx);
            fs.writeFileSync(spaceFile, newContent, 'utf8');
            console.log('Fixed duplicates');
        } else { console.log('Cannot find afterEndIdx'); }
    } else { console.log('Cannot find endBlockStart'); }
} else { console.log('Cannot find startIdx'); }
