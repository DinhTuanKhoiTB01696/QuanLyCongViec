const fs = require('fs');
const path = require('path');

const spaceFile = path.join('d:', 'A', 'QuanLyCongViec', 'Frontend', 'src', 'views', 'SpaceSummary.vue');
let content = fs.readFileSync(spaceFile, 'utf8');

const tStr = '<!-- TAB CONTENT: BOARD (Kanban) -->';
const startIdx = content.indexOf(tStr);
if (startIdx > -1) {
    const endStr = '<!-- TAB CONTENT: SUMMARY (Grid Charts) -->';
    const endIdx = content.indexOf(endStr, startIdx);
    if (endIdx > -1) {
        console.log(content.substring(startIdx, startIdx + 2000));
    }
}
