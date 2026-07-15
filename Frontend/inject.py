import sys

def modify():
    with open('src/components/TaskDetailModal.vue', 'r', encoding='utf-8') as f:
        content = f.read()
        
    with open('contingency_html.txt', 'r', encoding='utf-8') as f:
        html_content = f.read()
        
    with open('contingency_modals.txt', 'r', encoding='utf-8') as f:
        modals = f.read()
        
    with open('contingency_js.txt', 'r', encoding='utf-8') as f:
        js_content = f.read()

    # 1. Add tab button
    # Search for activity-tabs-wrapper
    idx = content.find('<div class="activity-tabs-wrapper">')
    if idx != -1:
        end_idx = content.find('</div>', idx)
        if end_idx != -1:
            wrapper = content[idx:end_idx+6]
            new_wrapper = wrapper.replace('</div>', '  <button class="activity-tab-btn" :class="{ active: activityTab === \'contingency\' }" @click="activityTab = \'contingency\'">K? ho?ch d? ph?ng</button>\n                 </div>')
            content = content.replace(wrapper, new_wrapper)
            print("Added tab button.")

    # 2. Add Contingency HTML
    # First update the history feed v-if
    content = content.replace('<div v-if="activityEntries.length" class="activity-feed">', '<div v-if="activityEntries.length && activityTab !== \'contingency\'" class="activity-feed">')
    
    # Then replace the empty state block
    empty_idx = content.find('class="activity-empty-state"')
    if empty_idx != -1:
        div_start = content.rfind('<div', 0, empty_idx)
        div_end = content.find('</div>', empty_idx) + 6
        empty_state = content[div_start:div_end]
        
        new_empty = html_content + '\n              ' + empty_state.replace('v-else', 'v-else-if="activityTab !== \'contingency\'"')
        content = content.replace(empty_state, new_empty)
        print("Injected HTML content.")

    # 3. Add modals
    last_template = content.rfind('</template>')
    if last_template != -1:
        content = content[:last_template] + '\n' + modals + '\n' + content[last_template:]
        print("Appended modals.")

    # 4. Inject JS logic
    js_start = content.find('// Comments logic')
    if js_start != -1:
        content = content[:js_start] + js_content + '\n\n' + content[js_start:]
        print("Injected JS logic.")
        
    # 5. Fix watch selectedTask
    content = content.replace('taskDependencies.value = [];', 'taskDependencies.value = [];\n      contingencyPlans.value = [];\n      showContingencyForm.value = false;\n      contingencyPlanForm.value = { name: \'\', riskLevel: \'Low\', riskStatus: \'Safe\', activationCondition: \'\', notes: \'\' };')
    print("Fixed watcher.")

    with open('src/components/TaskDetailModal.vue', 'w', encoding='utf-8') as f:
        f.write(content)
        
modify()
