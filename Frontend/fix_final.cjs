const fs = require('fs');
const path = require('path');
const file = 'src/components/TaskDetailModal.vue';
let content = fs.readFileSync(file, 'utf8');

// 1. Replace the Contingency Plan UI Tab Content
const startTabStr = '<!-- CONTINGENCY PLAN UI -->';
const endTabStr = '<div class="comment-box mb-6" v-if="activityTab === ' + "'" + 'comments' + "'" + '">';

const startTabIdx = content.indexOf(startTabStr);
const endTabIdx = content.indexOf(endTabStr);

if (startTabIdx === -1 || endTabIdx === -1) {
    console.error('Tab bounds not found', startTabIdx, endTabIdx);
    process.exit(1);
}

const newTabContent = `<!-- CONTINGENCY PLAN UI -->
            <div class="mb-6 contingency-plans-container" v-if="activityTab === 'contingency'">
              <div v-if="isLoadingContingency" class="text-center py-6 text-muted">
                 <i class="fa-solid fa-spinner fa-spin text-2xl"></i>
                 <div class="mt-2 text-sm font-medium">Đang tải kế hoạch...</div>
              </div>
              <div v-else>
                  <div class="flex justify-between items-center mb-4">
                     <h3 class="text-[14px] font-bold text-[var(--color-text-primary)]">Danh sách Kế hoạch Dự phòng</h3>
                     <button class="s-btn s-btn-primary px-4 py-2 text-[13px] font-bold rounded-lg shadow-sm hover:shadow-md transition-all flex items-center" @click="openCreateContingencyForm" v-if="contingencyPlans.length > 0">
                        <i class="fa-solid fa-plus mr-2"></i> Thêm Kế hoạch
                     </button>
                  </div>
                  
                  <div v-if="contingencyPlans.length > 0" class="flex flex-col gap-4">
                     <div class="contingency-card group hover:border-[var(--color-accent)] transition-all duration-300 relative overflow-hidden bg-[var(--bg-secondary)] border border-[var(--border-color)] rounded-xl p-5" v-for="plan in contingencyPlans" :key="plan.id">
                        
                        <div class="flex justify-between items-start mb-4">
                           <div>
                              <h4 class="text-[16px] font-bold text-[var(--color-text-primary)] mb-1.5 flex items-center cursor-pointer hover:text-[var(--color-accent)] transition-colors" @click="openContingencyDetail(plan)">
                                 <i class="fa-solid fa-shield-halved text-[var(--color-text-muted)] mr-2 text-sm"></i>
                                 {{ plan.name }}
                              </h4>
                              <div class="flex flex-wrap items-center gap-2 mt-1">
                                 <span class="text-[10px] px-2.5 py-1 rounded-md font-bold tracking-wide uppercase border shadow-sm" :class="getRiskLevelClass(plan.riskLevel)">{{ plan.riskLevel }}</span>
                              </div>
                           </div>
                           <div class="flex gap-1" v-if="!plan.isActivated">
                              <el-dropdown trigger="click" @command="(cmd) => handleContingencyAction(cmd, plan)">
                                 <button class="text-[var(--color-text-muted)] hover:bg-[var(--bg-tertiary)] hover:text-[var(--color-text-primary)] w-8 h-8 rounded-lg flex items-center justify-center transition-all bg-transparent">
                                    <i class="fa-solid fa-ellipsis-vertical text-lg"></i>
                                 </button>
                                 <template #dropdown>
                                    <el-dropdown-menu class="theme-dropdown shadow-lg rounded-xl overflow-hidden border border-[var(--border-color)]">
                                       <el-dropdown-item command="edit" class="py-2.5 px-4 font-medium"><i class="fa-solid fa-pen mr-2 text-[var(--color-text-muted)]"></i> Chỉnh sửa</el-dropdown-item>
                                       <el-dropdown-item command="delete" class="py-2.5 px-4 font-medium text-red-600 hover:bg-red-50"><i class="fa-solid fa-trash mr-2"></i> Xóa kế hoạch</el-dropdown-item>
                                    </el-dropdown-menu>
                                 </template>
                              </el-dropdown>
                           </div>
                        </div>

                        <div class="bg-[var(--bg-primary)] rounded-lg p-3.5 mb-4 border border-[var(--border-color)]/60" v-if="plan.riskDescription">
                           <div class="text-[11px] font-bold text-[var(--color-text-muted)] uppercase tracking-wider mb-1.5 flex items-center"><i class="fa-solid fa-triangle-exclamation mr-1.5 text-[var(--color-danger)]"></i> Rủi ro (Cho rủi ro gì)</div>
                           <div class="text-[13px] text-[var(--color-text-primary)] font-medium leading-relaxed">
                              {{ plan.riskDescription }}
                           </div>
                        </div>

                        <!-- Danh sách Task dự phòng -->
                        <div class="mt-4">
                           <div class="flex justify-between items-center mb-2">
                               <div class="text-[12px] font-bold text-[var(--color-accent)] uppercase tracking-wider flex items-center"><i class="fa-solid fa-list-check mr-1.5"></i> Task Dự Phòng ({{ plan.contingencyPlanTasks?.length || 0 }})</div>
                               <button class="text-[11px] font-bold text-[var(--color-accent)] hover:underline" @click="openCreateTaskForm(plan.id)" v-if="!plan.isActivated">+ Thêm Task</button>
                           </div>
                           <div v-if="plan.contingencyPlanTasks && plan.contingencyPlanTasks.length > 0" class="flex flex-col gap-2">
                               <div v-for="task in plan.contingencyPlanTasks" :key="task.id" class="bg-[var(--bg-primary)] p-3 rounded-lg border border-[var(--border-color)] flex justify-between items-center">
                                  <div>
                                     <div class="font-semibold text-[13px] text-[var(--color-text-primary)]">{{ task.title }}</div>
                                     <div class="text-[11px] text-[var(--color-text-muted)] mt-1">Trạng thái: <span class="font-bold uppercase tracking-wider" :style="{color: getStatusColor(task.statusName)}">{{ task.statusName }}</span></div>
                                  </div>
                                  <div v-if="!task.isActivated && !plan.isActivated">
                                     <button class="text-[11px] px-3 py-1.5 rounded bg-[var(--color-accent)] text-white font-bold hover:shadow-md transition-all active:scale-95" @click="confirmActivateTask(plan, task)">
                                        Kích hoạt
                                     </button>
                                  </div>
                                  <div v-else-if="task.isActivated" class="text-[11px] font-bold text-green-600 flex items-center bg-green-50 px-2 py-1 rounded">
                                     <i class="fa-solid fa-check mr-1"></i> Đã kích hoạt
                                  </div>
                               </div>
                           </div>
                           <div v-else class="text-[12px] text-[var(--color-text-muted)] italic py-2">Chưa có task dự phòng nào.</div>
                        </div>

                     </div>
                  </div>
                  
                  <div v-else class="text-center py-16 px-4 text-[var(--color-text-muted)] border-2 border-dashed border-[var(--border-color)]/70 hover:border-[var(--color-accent)]/50 transition-colors rounded-2xl bg-[var(--bg-secondary)] flex flex-col items-center justify-center group cursor-pointer" @click="openCreateContingencyForm">
                     <div class="w-16 h-16 rounded-full bg-[var(--bg-primary)] shadow-sm flex items-center justify-center mb-5 group-hover:scale-110 transition-transform duration-300">
                        <i class="fa-solid fa-shield-halved text-3xl text-[var(--color-text-muted)] group-hover:text-[var(--color-accent)] transition-colors"></i>
                     </div>
                     <h4 class="text-[var(--color-text-primary)] text-lg font-bold mb-2">Chưa có kế hoạch dự phòng</h4>
                     <p class="text-[13px] opacity-80 mb-6 max-w-[320px] leading-relaxed">Một kế hoạch dự phòng giúp nhóm chuẩn bị sẵn các phương án fallback tự động khi công việc gặp rủi ro.</p>
                     <button class="s-btn s-btn-primary px-6 py-2.5 text-[13px] font-bold rounded-lg shadow-md hover:shadow-lg transition-all flex items-center">
                        <i class="fa-solid fa-plus mr-2"></i> Tạo kế hoạch đầu tiên
                     </button>
                  </div>
               </div>
            </div>
            
            `;

content = content.substring(0, startTabIdx) + newTabContent + content.substring(endTabIdx);

// 2. Replace Modal Dialogs
const startModalIdx = content.indexOf('<!-- CONTINGENCY DIALOGS -->');
const endModalIdx = content.lastIndexOf('</template>');
if (startModalIdx === -1 || endModalIdx === -1) {
    console.error('Modal bounds not found');
    process.exit(1);
}

const newModalContent = `<!-- CONTINGENCY DIALOGS -->
<el-dialog v-model="showContingencyForm" :title="editingContingencyPlanId ? 'Cập nhật Kế hoạch dự phòng' : 'Thêm Kế hoạch dự phòng mới'" width="600px" class="theme-dialog" :teleported="false">
   <div class="cm-form-group">
      <label class="cm-label">Tên Kế hoạch <span class="text-red-500">*</span></label>
      <input type="text" class="cm-inputbox" v-model="contingencyPlanForm.name" placeholder="Ví dụ: Kế hoạch fallback máy chủ" />
   </div>
   <div class="cm-form-group mt-3">
      <label class="cm-label">Rủi ro (Cho rủi ro gì) <span class="text-red-500">*</span></label>
      <textarea class="cm-textareabox h-[80px]" v-model="contingencyPlanForm.riskDescription" placeholder="Ví dụ: Khi server chính vượt quá 95% CPU..."></textarea>
   </div>
   <div class="cm-form-group mt-3">
      <label class="cm-label">Mức độ nguy hiểm</label>
      <el-select v-model="contingencyPlanForm.riskLevel" class="w-full" :teleported="false">
         <el-option label="Critical (Nghiêm trọng)" value="Critical" />
         <el-option label="High (Cao)" value="High" />
         <el-option label="Medium (Trung bình)" value="Medium" />
         <el-option label="Low (Thấp)" value="Low" />
      </el-select>
   </div>
   <div class="cm-form-group mt-3">
      <label class="cm-label">Ghi chú (Rich text)</label>
      <div class="cm-editor-wrapper bg-[var(--bg-secondary)] min-h-[150px] p-2 border border-[var(--color-border)] rounded">
         <editor-content :editor="recoveryPlanEditor" />
      </div>
   </div>
   <template #footer>
      <div class="flex justify-end gap-2 mt-4">
         <button class="s-btn hover:bg-gray-100 dark:hover:bg-gray-700 rounded px-4 py-2" @click="showContingencyForm = false">Hủy</button>
         <button class="s-btn s-btn-primary rounded px-4 py-2" @click="saveContingencyPlan" :disabled="isSavingContingency">
            <i class="fa-solid fa-spinner fa-spin mr-2" v-if="isSavingContingency"></i>
            {{ editingContingencyPlanId ? 'Cập nhật' : 'Tạo kế hoạch' }}
         </button>
      </div>
   </template>
</el-dialog>

<!-- Thêm Task Dự Phòng Dialog -->
<el-dialog v-model="showTaskForm" title="Thêm Task Dự Phòng" width="500px" class="theme-dialog" :teleported="false">
   <div class="cm-form-group">
      <label class="cm-label">Tên Task <span class="text-red-500">*</span></label>
      <input type="text" class="cm-inputbox" v-model="taskForm.title" placeholder="Ví dụ: Khởi động server backup" />
   </div>
   <div class="cm-form-group mt-3">
      <label class="cm-label">Mô tả chi tiết</label>
      <textarea class="cm-textareabox h-[80px]" v-model="taskForm.description" placeholder="Mô tả công việc cần làm..."></textarea>
   </div>
   <div class="grid grid-cols-2 gap-4 mt-3">
      <div class="cm-form-group">
         <label class="cm-label">Độ ưu tiên</label>
         <el-select v-model="taskForm.priority" class="w-full" :teleported="false">
            <el-option label="1 (Cao nhất)" :value="1" />
            <el-option label="2 (Cao)" :value="2" />
            <el-option label="3 (Trung bình)" :value="3" />
            <el-option label="4 (Thấp)" :value="4" />
         </el-select>
      </div>
      <div class="cm-form-group">
         <label class="cm-label">Người thực hiện</label>
         <el-select v-model="taskForm.assigneeId" class="w-full" placeholder="Chọn người..." filterable clearable :teleported="false">
            <el-option v-for="user in projectMembers" :key="user.id" :label="user.fullName || user.email" :value="user.id" />
         </el-select>
      </div>
   </div>
   <template #footer>
      <div class="flex justify-end gap-2 mt-4">
         <button class="s-btn hover:bg-gray-100 dark:hover:bg-gray-700 rounded px-4 py-2" @click="showTaskForm = false">Hủy</button>
         <button class="s-btn s-btn-primary rounded px-4 py-2" @click="saveContingencyTask" :disabled="isSavingTask">
            <i class="fa-solid fa-spinner fa-spin mr-2" v-if="isSavingTask"></i>
            Tạo Task
         </button>
      </div>
   </template>
</el-dialog>

<el-drawer v-model="showContingencyDetail" size="500px" title="Chi tiết Kế hoạch dự phòng" class="theme-drawer" :teleported="false">
   <div v-if="viewingContingencyPlan" class="p-5 flex flex-col gap-6">
      <div>
         <h2 class="text-xl font-bold text-[var(--color-text-primary)] mb-2">{{ viewingContingencyPlan.name }}</h2>
         <div class="flex flex-wrap gap-2">
            <span class="text-[11px] px-3 py-1 rounded-full font-bold uppercase tracking-wider border shadow-sm" :class="getRiskLevelClass(viewingContingencyPlan.riskLevel)">Risk: {{ viewingContingencyPlan.riskLevel }}</span>
         </div>
      </div>
      
      <div class="bg-[var(--bg-secondary)] rounded-xl p-4 border border-[var(--border-color)]">
         <div class="text-[12px] font-bold text-[var(--color-danger)] uppercase tracking-wider mb-2 flex items-center"><i class="fa-solid fa-triangle-exclamation mr-2"></i> Rủi ro (Cho rủi ro gì)</div>
         <p class="text-[14px] text-[var(--color-text-primary)] leading-relaxed">{{ viewingContingencyPlan.riskDescription }}</p>
      </div>

      <div v-if="viewingContingencyPlan.notes">
         <div class="text-[13px] font-bold text-[var(--color-text-primary)] uppercase tracking-wider mb-3">Ghi chú</div>
         <div class="prose prose-sm dark:prose-invert max-w-none text-[14px] leading-relaxed p-4 bg-[var(--bg-secondary)] rounded-xl border border-[var(--border-color)]" v-html="viewingContingencyPlan.notes"></div>
      </div>
      
      <div class="bg-[var(--color-accent)]/5 rounded-xl p-4 border border-[var(--color-accent)]/20">
         <div class="text-[12px] font-bold text-[var(--color-accent)] uppercase tracking-wider mb-2 flex items-center"><i class="fa-solid fa-list-check mr-2"></i> Danh sách Task dự phòng</div>
         <div v-if="viewingContingencyPlan.contingencyPlanTasks && viewingContingencyPlan.contingencyPlanTasks.length > 0" class="flex flex-col gap-2">
            <div v-for="task in viewingContingencyPlan.contingencyPlanTasks" :key="task.id" class="bg-[var(--bg-primary)] p-3 rounded-lg border border-[var(--border-color)]">
               <div class="font-semibold text-[13px] text-[var(--color-text-primary)]">{{ task.title }}</div>
               <div class="text-[11px] text-[var(--color-text-muted)] mt-1">Trạng thái: <span class="font-bold uppercase tracking-wider">{{ task.statusName }}</span></div>
            </div>
         </div>
         <div v-else class="text-sm italic text-[var(--color-text-muted)]">Chưa liên kết Task.</div>
      </div>
   </div>
</el-drawer>
\n`;

content = content.substring(0, startModalIdx) + newModalContent + content.substring(endModalIdx);

// 3. Replace JS logic
const startJsIdx = content.indexOf('// Contingency Plan logic');
const endJsIdx = content.indexOf('// Comments logic');
if (startJsIdx === -1 || endJsIdx === -1) {
    console.error('JS bounds not found');
    process.exit(1);
}

const newJsContent = `// Contingency Plan logic
const contingencyPlans = ref([]);
const showContingencyForm = ref(false);
const editingContingencyPlanId = ref(null);
const contingencyPlanForm = ref({ name: '', riskLevel: 'Low', riskDescription: '', notes: '' });
const recoveryPlanEditor = ref(null);
const isLoadingContingency = ref(false);
const isSavingContingency = ref(false);

const showContingencyDetail = ref(false);
const viewingContingencyPlan = ref(null);

// Task logic
const showTaskForm = ref(false);
const isSavingTask = ref(false);
const activePlanIdForTask = ref(null);
const taskForm = ref({ title: '', description: '', priority: 3, assigneeId: null });

function getRiskLevelClass(level) {
  switch (level) {
    case 'Critical': return 'border-red-300 bg-red-100 text-red-800';
    case 'High': return 'border-orange-300 bg-orange-100 text-orange-800';
    case 'Medium': return 'border-yellow-300 bg-yellow-100 text-yellow-800';
    default: return 'border-blue-300 bg-blue-100 text-blue-800';
  }
}
function getStatusColor(statusName) {
   switch (statusName) {
      case 'Đang chờ xử lý': return '#F59E0B';
      case 'IN PROGRESS': return '#3B82F6';
      default: return '#6B7280';
   }
}

async function fetchContingencyPlans() {
  if (!props.selectedTask || !props.selectedTask.id) return;
  isLoadingContingency.value = true;
  try {
    const res = await axiosClient.get(\`/worktasks/\${props.selectedTask.id}/contingency-plans\`);
    contingencyPlans.value = res.data?.data || [];
  } catch (err) {
    console.error('Lỗi load contingency plans:', err);
    contingencyPlans.value = [];
  } finally {
    isLoadingContingency.value = false;
  }
}

function openCreateContingencyForm() {
  editingContingencyPlanId.value = null;
  contingencyPlanForm.value = { name: '', riskLevel: 'Low', riskDescription: '', notes: '' };
  if (!recoveryPlanEditor.value) {
    recoveryPlanEditor.value = new Editor({
      extensions: [StarterKit],
      content: '',
      onUpdate: () => {
         contingencyPlanForm.value.notes = recoveryPlanEditor.value.getHTML();
      }
    });
  } else {
    recoveryPlanEditor.value.commands.setContent('');
  }
  showContingencyForm.value = true;
}

async function saveContingencyPlan() {
  if (!contingencyPlanForm.value.name || !contingencyPlanForm.value.riskDescription) {
     ElMessage.warning('Vui lòng điền Tên kế hoạch và Rủi ro.');
     return;
  }
  isSavingContingency.value = true;
  try {
    const payload = {
       name: contingencyPlanForm.value.name,
       riskLevel: contingencyPlanForm.value.riskLevel,
       riskDescription: contingencyPlanForm.value.riskDescription,
       notes: contingencyPlanForm.value.notes
    };
    if (editingContingencyPlanId.value) {
       await axiosClient.put(\`/worktasks/\${props.selectedTask.id}/contingency-plans/\${editingContingencyPlanId.value}\`, payload);
       ElMessage.success('Đã cập nhật kế hoạch dự phòng');
    } else {
       await axiosClient.post(\`/worktasks/\${props.selectedTask.id}/contingency-plans\`, payload);
       ElMessage.success('Đã tạo kế hoạch dự phòng mới');
    }
    showContingencyForm.value = false;
    await fetchContingencyPlans();
    fetchAuditTimeline();
  } catch (err) {
    ElMessage.error(err.response?.data?.message || 'Lỗi khi lưu kế hoạch dự phòng');
  } finally {
    isSavingContingency.value = false;
  }
}

function openContingencyDetail(plan) {
   viewingContingencyPlan.value = plan;
   showContingencyDetail.value = true;
}

function handleContingencyAction(cmd, plan) {
   if (cmd === 'edit') {
      editingContingencyPlanId.value = plan.id;
      contingencyPlanForm.value = { 
         name: plan.name, 
         riskLevel: plan.riskLevel, 
         riskDescription: plan.riskDescription || '',
         notes: plan.notes
      };
      if (!recoveryPlanEditor.value) {
         recoveryPlanEditor.value = new Editor({
            extensions: [StarterKit],
            content: plan.notes || '',
            onUpdate: () => {
               contingencyPlanForm.value.notes = recoveryPlanEditor.value.getHTML();
            }
         });
      } else {
         recoveryPlanEditor.value.commands.setContent(plan.notes || '');
      }
      showContingencyForm.value = true;
   } else if (cmd === 'delete') {
      ElMessageBox.confirm('Bạn có chắc chắn muốn xóa kế hoạch dự phòng này?', 'Xác nhận xóa', {
         confirmButtonText: 'Xóa',
         cancelButtonText: 'Hủy',
         type: 'warning'
      }).then(async () => {
         try {
            await axiosClient.delete(\`/worktasks/\${props.selectedTask.id}/contingency-plans/\${plan.id}\`);
            ElMessage.success('Đã xóa kế hoạch dự phòng');
            await fetchContingencyPlans();
         } catch (err) {
            ElMessage.error('Xóa thất bại');
         }
      }).catch(() => {});
   }
}

function openCreateTaskForm(planId) {
   activePlanIdForTask.value = planId;
   taskForm.value = { title: '', description: '', priority: 3, assigneeId: null };
   showTaskForm.value = true;
}

async function saveContingencyTask() {
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
}

function confirmActivateTask(plan, task) {
   ElMessageBox.confirm(
      \`<div class="mb-2">Bạn muốn kích hoạt Task dự phòng này?</div><div class="p-2 bg-gray-100 rounded text-sm mb-2"><b>Task:</b> \${task.title}</div><div class="text-xs text-gray-500">Sau khi kích hoạt, Task dự phòng sẽ chuyển sang trạng thái hoạt động (In Progress) và được giao cho người phụ trách.</div>\`,
      'Xác nhận kích hoạt Task',
      {
         confirmButtonText: 'Kích hoạt',
         cancelButtonText: 'Hủy',
         dangerouslyUseHTMLString: true,
         type: 'warning'
      }
   ).then(async () => {
      try {
         await axiosClient.post(\`/worktasks/\${props.selectedTask.id}/contingency-plans/\${plan.id}/tasks/\${task.id}/activate\`);
         ElMessage.success('Task dự phòng đã được kích hoạt thành công');
         await fetchContingencyPlans();
         fetchAuditTimeline();
      } catch (err) {
         ElMessage.error(err.response?.data?.message || 'Kích hoạt thất bại');
      }
   }).catch(() => {});
}

watch(activityTab, (newTab) => {
   if (newTab === 'contingency') {
      fetchContingencyPlans();
   }
});

\n`;

content = content.substring(0, startJsIdx) + newJsContent + content.substring(endJsIdx);

fs.writeFileSync(file, content, 'utf8');
console.log('Done rewriting TaskDetailModal.vue');
