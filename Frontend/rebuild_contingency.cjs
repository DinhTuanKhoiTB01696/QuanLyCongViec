const fs = require('fs');
const path = require('path');
const file = './src/components/TaskDetailModal.vue';
let content = fs.readFileSync(file, 'utf8');

const htmlContent = fs.readFileSync('contingency_html.txt', 'utf8');
const modalsContent = fs.readFileSync('contingency_modals.txt', 'utf8');
const jsContent = fs.readFileSync('contingency_js.txt', 'utf8');

// 1. Add Contingency Tab Button
const tabWrapperRegex = /<div class="activity-tabs-wrapper">\s*<button class="activity-tab-buttn" :class="\{ active: activityTab === 'comments' \}" @click="activityTab = 'comments'">[^<]+<\/button>\s*<button class="activity-tab-btn" :class="\{ active: activityTab === 'history' \}" @click="activityTab = 'history'">[^<]+<\/button>\s*<\/div>/;
if (tabWrapperRegex.test(content)) {
    const tabMatch = content.match(tabWrapperRegex)[0];
    const newTabMatch = tabMatch.replace('</div>', '  <button class="activity-tab-btn" :class="{ active: activityTab === \'contingency\' }" @click="activityTab = \'contingency\'">Kế hoạch dự phòng</button>\n                 </div>');
    content = content.replace(tabMatch, newTabMatch);
    console.log('Added Contingency Tab Button.');
} else {
    console.log('Failed to find tab wrapper!');
}

// 2. Add Contingency HTML Content
const emptyStateRegex = /<div v-else class="activity-empty-state">Chưa có \{\{ activityTab === 'comments' \? 'bình luận' : 'hoạt động' \}\} nào\.<\/div>/;
const emptyStateRegex2 = /<div v-else class="activity-empty-state">ChÆ@a có \{\{ activityTab === 'comments' \? 'bÀ°nh luạn' : 'hoÆA!t Ä'’ng' \}\} nÀo\.<\/div>/;

if (emptyStateRegex.test(content) || emptyStateRegex2.test(content)) {
    const regexToUse = emptyStateRegex.test(content) ? emptyStateRegex : emptyStateRegex2;
    const match = content.match(regexToUse)[0];
    const historyFeedRegex = /<div v-if="activityEntries\.length" class="activity-feed">/;
    if (historyFeedRegex.test(content)) {
        content = content.replace(historyFeedRegex, '<div v-if="activityEntries.length && activityTab !== \'contingency\'" class="activity-feed">');
    }
    const newEmptyState = htmlContent + '\n              <div v-else-if="activityTab !== \'contingency\'" class="activity-empty-state">Chưa có {{ activityTab === \'comments\' ? \'bình luận\' : \'hoạt động\' }} nào.</div>';
    content = content.replace(match, newEmptyState);
    console.log('Injected Contingency HTML content.');
} else {
    console.log('Failed to find empty state!');
}

// 3. Append Modals
const lastTemplateIdx = content.lastIndexOf('</template>');
if (lastTemplateIdx !== -1) {
    content = content.substring(0, lastTemplateIdx) + '\n' + modalsContent + '\n' + content.substring(lastTemplateIdx);
    console.log('Appended Modals to template.');
}

// 4. Inject JS logic
const jsStartStr = '// Comments logic';
const jsStartIdx = content.indexOf(jsStartStr);
if (jsStartIdx !== -1) {
    content = content.substring(0, jsStartIdx) + jsContent + '\n\n' + content.substring(jsStartIdx);
    console.log('Injected JS Logic.');
} else {
    console.log('Failed to find JS logic insertion point!');
}

// 5. Update watch selectedTask to reset state
const watchRegex = /taskDependencies\.value = \[\];/;
if (watchRegex.test(content)) {
    content = content.replace(watchRegex, 'taskDependencies.value = [];\n      contingencyPlans.value = [];\n      showContingencyForm.value = false;\n      contingencyPlanForm.value = { name: \'\', riskLevel: \'Low\', riskStatus: \'Safe\', activationCondition: \'\', notes: \'\' };');
    console.log('Updated watch selectedTask logic.');
}

fs.writeFileSync(file, content, 'utf8');
console.log('Rebuild Complete.');