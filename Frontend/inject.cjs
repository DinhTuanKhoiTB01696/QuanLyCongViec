const fs = require('fs');
const content = fs.readFileSync('src/components/TaskDetailModal.vue', 'utf8');
const htmlContent = fs.readFileSync('contingency_html.txt', 'utf8');
const modalsContent = fs.readFileSync('contingency_modals.txt', 'utf8');
const jsContent = fs.readFileSync('contingency_js.txt', 'utf8');

let newContent = content;

const tabIdx = newContent.indexOf('<div class="activity-tabs-wrapper">');
if(tabIdx !== -1) {
    const tabEndIdx = newContent.indexOf('</div>', tabIdx);
    const wrapper = newContent.substring(tabIdx, tabEndIdx + 6);
    const newWrapper = wrapper.replace('</div>', '  <button class="activity-tab-btn" :class="{ active: activityTab === \'contingency\' }" @click="activityTab = \'contingency\'">Kế hoạch dự phòng</button>\n                 </div>');
    newContent = newContent.replace(wrapper, newWrapper);
    console.log('Tabs updated');
}

newContent = newContent.replace('<div v-if="activityEntries.length" class="activity-feed">', '<div v-if="activityEntries.length && activityTab !== \'contingency\'" class="activity-feed">');

const emptyIdx = newContent.indexOf('class="activity-empty-state"');
if(emptyIdx !== -1) {
    const startDiv = newContent.lastIndexOf('<div', emptyIdx);
    const endDiv = newContent.indexOf('</div>', emptyIdx) + 6;
    const emptyState = newContent.substring(startDiv, endDiv);
    const newEmpty = htmlContent + '\n              ' + emptyState.replace('v-else', 'v-else-if="activityTab !== \'contingency\'"');
    newContent = newContent.replace(emptyState, newEmpty);
    console.log('HTML injected');
}

const lastTemplate = newContent.lastIndexOf('</template>');
if(lastTemplate !== -1) {
    newContent = newContent.substring(0, lastTemplate) + '\n' + modalsContent + '\n' + newContent.substring(lastTemplate);
    console.log('Modals appended');
}

const jsStart = newContent.indexOf('// Comments logic');
if(jsStart !== -1) {
    newContent = newContent.substring(0, jsStart) + jsContent + '\n\n' + newContent.substring(jsStart);
    console.log('JS injected');
}

newContent = newContent.replace('taskDependencies.value = [];', 'taskDependencies.value = [];\n      contingencyPlans.value = [];\n      showContingencyForm.value = false;\n      contingencyPlanForm.value = { name: \'\', riskLevel: \'Low\', riskStatus: \'Safe\', activationCondition: \'\', notes: \'\' };');

fs.writeFileSync('src/components/TaskDetailModal.vue', newContent, 'utf8');