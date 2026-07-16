const fs = require('fs');
const file = 'src/components/TaskDetailModal.vue';
let content = fs.readFileSync(file, 'utf8');

content = content.replace(
  "await axiosClient.post(`/worktasks/${props.selectedTask.id}/contingency-plans/${activePlanIdForTask.value}/tasks`, taskForm.value);",
  "await axiosClient.post(`/worktasks/${props.selectedTask.id}/contingency-plans/${activePlanIdForTask.value}/tasks/create`, taskForm.value);"
);

// Fix user.id to user.userId
content = content.replace(
  "@click=\"taskForm.assigneeId = user.id\"",
  "@click=\"taskForm.assigneeId = user.userId\""
);
content = content.replace(
  "taskForm.assigneeId === user.id",
  "taskForm.assigneeId === user.userId"
);
// Also fix the display text for assignee
content = content.replace(
  "projectMembers.find(m => m.id === taskForm.assigneeId)",
  "projectMembers.find(m => m.userId === taskForm.assigneeId)"
);

fs.writeFileSync(file, content, 'utf8');
