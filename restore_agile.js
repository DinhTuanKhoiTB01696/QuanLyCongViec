const fs = require('fs');
const spaceFile = 'd:\\A\\QuanLyCongViec\\Frontend\\src\\views\\SpaceSummary.vue';
let content = fs.readFileSync(spaceFile, 'utf8');

// Fix 1: reporterName -> assigneeName
content = content.replace(
  'if (filterByAssignee && t.reporterName !== filterByAssignee) return false;',
  'if (filterByAssignee && t.assigneeName !== filterByAssignee) return false;'
);

// Fix 2: backlogTaskGroups
const tGroupIndex = content.indexOf('const taskGroups = computed(() => {');
if (tGroupIndex > -1) {
    const backlogProps = `
const backlogTaskGroups = computed(() => {
  const allTasks = filteredTasks.value.filter(t => t.sprintId == null && t.statusName !== 'DONE');
  const gb = groupBy.value;
  const gOrder = groupByOrder.value;

  if (gb === 'status') {
    const map = { TODO: [], INPROGRESS: [], INREVIEW: [], DONE: [] };
    allTasks.forEach(t => {
      const s = (t.statusName || 'TO DO').toUpperCase().replace(/\s/g, '');
      if (map[s]) map[s].push(t);
      else map.TODO.push(t);
    });

    let groups = [
      { id: 'bgrp-todo', statusText: 'TO DO', statusBg: '#374151', statusColor: '#9ca3af', expanded: true, items: map.TODO },
      { id: 'bgrp-progress', statusText: 'IN PROGRESS', statusBg: '#6b21a8', statusColor: '#ffffff', expanded: true, items: map.INPROGRESS },
      { id: 'bgrp-review', statusText: 'IN REVIEW', statusBg: '#0369a1', statusColor: '#ffffff', expanded: true, items: map.INREVIEW }
    ];
    if (gOrder === 'desc') groups.reverse();
    return groups;
  }
  return [];
});

`;
    content = content.substring(0, tGroupIndex) + backlogProps + content.substring(tGroupIndex);
    
    // update the template
    content = content.replace(
      '<div v-for="group in taskGroups" :key="group.id" class="backlog-group">',
      '<div v-for="group in backlogTaskGroups" :key="group.id" class="backlog-group">'
    );
}

// Fix 3: isValidStatusTransition
const dChangeIdx = content.indexOf('const handleDraggableChange = (evt, targetGroup) => {');
if (dChangeIdx > -1) {
    const dChangeNew = `
const isValidStatusTransition = (oldStatus, newStatus) => {
  const order = { 'TO DO': 0, 'IN PROGRESS': 1, 'IN REVIEW': 2, 'DONE': 3 };
  const o = order[oldStatus?.toUpperCase() || 'TO DO'];
  const n = order[newStatus?.toUpperCase() || 'TO DO'];
  if (o === undefined || n === undefined) return true;
  if (n === o) return true;
  return Math.abs(n - o) <= 2; // Actually let's just use 1 to enforce strict, or 2. Prompt mentioned TO DO -> DONE is invalid (3).
};

const handleDraggableChange = (evt, targetGroup) => {
  if (evt.added) {
    const task = evt.added.element;
    const oldStatus = task.statusName;
    const newStatus = targetGroup?.statusText || 'TO DO';
    
    if (!isValidStatusTransition(oldStatus, newStatus)) {
      ElMessage.warning(\`Chuy?n tr?ng thái t? \${oldStatus} sang \${newStatus} không h?p l?.\`);
      return;
    }
  }
  // Let the API call handle the rest which we already had...
`;
    content = content.replace('const handleDraggableChange = (evt, targetGroup) => {', dChangeNew);
}

fs.writeFileSync(spaceFile, content);
console.log('Restored Agile Fixes!');
