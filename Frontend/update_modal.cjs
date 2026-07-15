const fs = require('fs');
const file = 'src/components/TaskDetailModal.vue';
let content = fs.readFileSync(file, 'utf8');

const startStr = '<!-- Thêm Task Dự Phòng Dialog (Style giống Tạo Công Việc Mới) -->';
const endStr = '</transition>';

let startIdx = content.indexOf(startStr);
let endIdx = content.indexOf(endStr, startIdx);

if (startIdx === -1 || endIdx === -1) {
    console.error('Bounds not found');
    process.exit(1);
}

const newModal = `<!-- Thêm Task Dự Phòng Dialog (Style giống Tạo Công Việc Mới) -->
<transition name="fade">
  <div class="task-modal-overlay" v-if="showTaskForm" @mousedown.self="showTaskForm = false" style="z-index: 999999;">
    <div class="create-centered-modal">
      <h3 class="cm-title">Thêm Task Dự Phòng</h3>
      
      <div class="cm-badge-row">
         <div class="cm-badge">
           <i class="fa-solid fa-shield-halved" style="color: #3b82f6"></i> TASK DỰ PHÒNG
         </div>
      </div>

      <div class="cm-form-group">
        <input type="text" class="cm-inputbox" placeholder="Tiêu đề" v-model="taskForm.title" />
        <textarea class="cm-textareabox" placeholder="Thêm mô tả..." v-model="taskForm.description"></textarea>
      </div>

      <div class="cm-toolbar-row">
         <!-- STATUS -->
         <div class="t-btn disabled"><i class="fa-regular fa-circle-dot" style="color: #F59E0B"></i> <span>Trạng thái</span> Cần làm</div>

         <!-- PRIORITY -->
         <el-dropdown trigger="click" @command="(cmd) => taskForm.priority = cmd">
           <div class="t-btn">
               <i class="fa-solid fa-angles-up text-red-500" v-if="taskForm.priority === 1"></i>
               <i class="fa-solid fa-chevron-up text-yellow-500" v-else-if="taskForm.priority === 2"></i>
               <i class="fa-solid fa-minus text-blue-500" v-else-if="taskForm.priority === 3"></i>
               <i class="fa-solid fa-arrow-down text-gray-500" v-else></i>
               <span>Độ ưu tiên</span> 
               {{ taskForm.priority === 1 ? 'Khẩn cấp' : (taskForm.priority === 2 ? 'Cao' : (taskForm.priority === 3 ? 'Trung bình' : 'Thấp')) }}
           </div>
           <template #dropdown>
             <el-dropdown-menu class="theme-dropdown">
               <el-dropdown-item :command="1"><i class="fa-solid fa-angles-up mr-2" style="color: #ef4444"></i> Khẩn cấp</el-dropdown-item>
               <el-dropdown-item :command="2"><i class="fa-solid fa-chevron-up mr-2" style="color: #f59e0b"></i> Cao</el-dropdown-item>
               <el-dropdown-item :command="3"><i class="fa-solid fa-minus mr-2" style="color: #3b82f6"></i> Trung bình</el-dropdown-item>
               <el-dropdown-item :command="4"><i class="fa-solid fa-arrow-down mr-2" style="color: var(--color-text-muted)"></i> Thấp</el-dropdown-item>
             </el-dropdown-menu>
           </template>
         </el-dropdown>

         <!-- ASSIGNEE -->
         <el-popover placement="bottom-start" trigger="click" popper-class="plane-popover" :width="220" @show="assigneeSearch = ''">
           <template #reference>
             <div class="t-btn"><i class="fa-regular fa-user"></i> <span>Người thực hiện</span> {{ projectMembers.find(m => m.id === taskForm.assigneeId)?.fullName || 'Người thực hiện' }}</div>
           </template>
           <div class="popover-content">
             <input type="text" v-model="assigneeSearch" class="popover-search" placeholder="Tìm người thực hiện..." />
             <div class="popover-list">
               <div class="popover-item flex items-center justify-between transition-colors cursor-pointer" v-for="user in projectMembers" :key="user.id" @click="taskForm.assigneeId = user.id">
                   <div class="flex items-center gap-2">
                       <span class="user-name">{{ user.fullName || user.email }}</span>
                   </div>
                   <i class="fa-solid fa-check text-blue-500" v-if="taskForm.assigneeId === user.id"></i>
               </div>
             </div>
           </div>
         </el-popover>
      </div>

      <div class="mt-4 flex justify-between items-center pt-4 border-t border-[var(--border-color)]">
         <div class="flex items-center gap-2">
            <el-switch v-model="createContingencyContinuously" />
            <span class="text-sm font-medium text-[var(--color-text-muted)]">Tạo liên tục</span>
         </div>
         <div class="flex gap-3">
             <button class="s-btn hover:bg-[var(--bg-tertiary)] rounded-full px-6 py-2 transition-all font-bold text-[var(--color-text-primary)]" @click="showTaskForm = false">Hủy</button>
             <button class="s-btn s-btn-primary rounded-full px-6 py-2 shadow-sm hover:shadow-md transition-all font-bold text-white flex items-center" @click="saveContingencyTask" :disabled="isSavingTask" style="background-color: #0ea5e9;">
                <i class="fa-solid fa-spinner fa-spin mr-2" v-if="isSavingTask"></i> Lưu
             </button>
         </div>
      </div>
    </div>
  </div>
`;

content = content.substring(0, startIdx) + newModal + content.substring(endIdx);

if (!content.includes('const createContingencyContinuously = ref(false);')) {
    const jsStart = content.indexOf('const showTaskForm = ref(false);');
    content = content.substring(0, jsStart) + 'const createContingencyContinuously = ref(false);\n' + content.substring(jsStart);
}

const oldSaveFunc = `async function saveContingencyTask() {
   if (!taskForm.value.title) {
      ElMessage.warning('Vui lòng điền tên Task');
      return;
   }
   isSavingTask.value = true;
   try {
      await axiosClient.post(\`/worktasks/\${props.selectedTask.id}/contingency-plans/\${activePlanIdForTask.value}/tasks\`, taskForm.value);
      ElMessage.success('Đã tạo Task dự phòng thành công');
      showTaskForm.value = false;
      await fetchContingencyPlans();
   } catch (err) {
      ElMessage.error(err.response?.data?.message || 'Tạo task thất bại');
   } finally {
      isSavingTask.value = false;
   }
}`;

const newSaveFunc = `async function saveContingencyTask() {
   if (!taskForm.value.title) {
      ElMessage.warning('Vui lòng điền tiêu đề');
      return;
   }
   isSavingTask.value = true;
   try {
      await axiosClient.post(\`/worktasks/\${props.selectedTask.id}/contingency-plans/\${activePlanIdForTask.value}/tasks\`, taskForm.value);
      ElMessage.success('Đã tạo Task dự phòng thành công');
      
      await fetchContingencyPlans();
      
      if (createContingencyContinuously.value) {
          taskForm.value = { title: '', description: '', priority: taskForm.value.priority, assigneeId: taskForm.value.assigneeId };
      } else {
          showTaskForm.value = false;
      }
   } catch (err) {
      ElMessage.error(err.response?.data?.message || 'Tạo task thất bại');
   } finally {
      isSavingTask.value = false;
   }
}`;

content = content.replace(oldSaveFunc, newSaveFunc);
fs.writeFileSync(file, content, 'utf8');
