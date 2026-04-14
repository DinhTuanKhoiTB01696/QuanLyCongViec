const fs = require('fs');
const spaceFile = 'd:\\A\\QuanLyCongViec\\Frontend\\src\\views\\SpaceSummary.vue';
const content = fs.readFileSync(spaceFile, 'utf8');

const sIdx = content.indexOf('const taskGroups = computed');
if (sIdx > -1) {
    console.log(content.substring(sIdx, sIdx + 1500));
}
