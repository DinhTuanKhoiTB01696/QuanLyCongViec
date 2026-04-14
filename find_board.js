const fs = require('fs');
const spaceFile = 'd:\\A\\QuanLyCongViec\\Frontend\\src\\views\\SpaceSummary.vue';
const content = fs.readFileSync(spaceFile, 'utf8');
const lines = content.split('\n');
for (let i = 0; i < lines.length; i++) {
    if (lines[i].includes('board-content')) {
        console.log(Found board-content at line );
    }
}
