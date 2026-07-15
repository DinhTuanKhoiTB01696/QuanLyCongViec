const fs = require('fs');
let content = fs.readFileSync('src/components/TaskDetailModal.vue', 'utf8');
content = content.replace(/<el-select v-model="contingencyPlanForm\.contingencyTaskId"[^>]+>/, '<el-select v-model="contingencyPlanForm.contingencyTaskId" filterable clearable placeholder="Chọn Task..." class="w-full" :teleported="false">');
fs.writeFileSync('src/components/TaskDetailModal.vue', content, 'utf8');