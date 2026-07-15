const fs = require('fs');
const file = 'src/components/TaskDetailModal.vue';
let content = fs.readFileSync(file, 'utf8');

const startStr = '<!-- Thêm Task Dự Phòng Dialog -->';
const endStr = '<el-drawer v-model="showContingencyDetail"';

const startIdx = content.indexOf(startStr);
const endIdx = content.indexOf(endStr);

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
        <input type="text" class="cm-inputbox" placeholder="Tên Task (Ví dụ: Khởi động server backup)" v-model="taskForm.title" />
        <textarea class="cm-textareabox" placeholder="Mô tả công việc cần làm..." v-model="taskForm.description"></textarea>
      </div>

      <div class="cm-toolbar-row">
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
         <el-select v-model="taskForm.assigneeId" class="w-[200px]" placeholder="Người thực hiện" filterable clearable :teleported="false">
            <template #prefix><i class="fa-regular fa-user"></i></template>
            <el-option v-for="user in projectMembers" :key="user.id" :label="user.fullName || user.email" :value="user.id" />
         </el-select>
      </div>

      <div class="mt-4 flex justify-between items-center pt-4 border-t border-[var(--border-color)]">
         <div class="text-xs text-gray-400 italic">* Task dự phòng sẽ mặc định ở trạng thái Đang chờ xử lý.</div>
         <div class="flex gap-2">
             <button class="s-btn hover:bg-gray-100 dark:hover:bg-gray-700 rounded-full px-6 py-2 border border-transparent hover:border-gray-300 transition-all font-bold" @click="showTaskForm = false">Hủy</button>
             <button class="s-btn s-btn-primary rounded-full px-6 py-2 shadow-md hover:shadow-lg transition-all font-bold text-white flex items-center" @click="saveContingencyTask" :disabled="isSavingTask" style="background-color: #0ea5e9;">
                <i class="fa-solid fa-spinner fa-spin mr-2" v-if="isSavingTask"></i> Tạo Task
             </button>
         </div>
      </div>
    </div>
  </div>
</transition>

`;

content = content.substring(0, startIdx) + newModal + content.substring(endIdx);
fs.writeFileSync(file, content, 'utf8');
console.log('Replaced Thêm Task Dự Phòng modal');
