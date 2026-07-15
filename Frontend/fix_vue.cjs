const fs = require('fs');

let content = fs.readFileSync('clean.txt', 'utf8');

// 1. Replace activity-tabs-wrapper (first occurrence)
content = content.replace(
  '<button class="activity-tab-btn" :class="{ active: activityTab === \'history\' }" @click="activityTab = \'history\'">Nhật ký hoạt động</button>\r\n                 </div>',
  '<button class="activity-tab-btn" :class="{ active: activityTab === \'history\' }" @click="activityTab = \'history\'">Nhật ký hoạt động</button>\n                 <button class="activity-tab-btn" :class="{ active: activityTab === \'contingency\' }" @click="activityTab = \'contingency\'">Kế hoạch dự phòng</button>\n                 </div>'
);

content = content.replace(
  '<button class="activity-tab-btn" :class="{ active: activityTab === \'history\' }" @click="activityTab = \'history\'">Nhật ký hoạt động</button>\n                 </div>',
  '<button class="activity-tab-btn" :class="{ active: activityTab === \'history\' }" @click="activityTab = \'history\'">Nhật ký hoạt động</button>\n                 <button class="activity-tab-btn" :class="{ active: activityTab === \'contingency\' }" @click="activityTab = \'contingency\'">Kế hoạch dự phòng</button>\n                 </div>'
);


// 2. Inject Contingency Plan UI
const historyEndIdx = content.indexOf('<div class="flex-center gap-2" v-if="activityTab === \'history\'">');
if (historyEndIdx > -1) {
    const nextDivIdx = content.indexOf('</div>', historyEndIdx);
    const nextNextDivIdx = content.indexOf('</div>', nextDivIdx + 6);
    
    const contingencyHtml = `
            <!-- CONTINGENCY PLAN UI -->
            <div class="mb-6 contingency-plans-container" v-else-if="activityTab === 'contingency'">
              <div v-if="isLoadingContingency" class="text-center py-6 text-muted">
                 <i class="fa-solid fa-spinner fa-spin text-2xl"></i>
                 <div class="mt-2 text-sm font-medium">Đang tải kế hoạch...</div>
              </div>
              <div v-else>
                  <div class="flex justify-between items-center mb-5">
                     <h3 class="sp-section-title mb-0 flex items-center text-[15px] font-bold"><i class="fa-solid fa-shield-halved text-[var(--color-warning)] mr-2 text-lg"></i> Kế hoạch dự phòng</h3>
                     <button class="s-btn s-btn-primary px-4 py-2 text-xs font-bold rounded shadow-sm" @click="openCreateContingencyForm" v-if="contingencyPlans.length > 0">
                        <i class="fa-solid fa-plus mr-1.5"></i> Thêm kế hoạch
                     </button>
                  </div>
                  
                  <!-- Danh sách Contingency Plans -->
                  <div v-if="contingencyPlans.length > 0" class="flex flex-col gap-4">
                     <div class="contingency-card group hover:shadow-lg transition-all duration-300 relative overflow-hidden" v-for="plan in contingencyPlans" :key="plan.id">
                        <!-- Card Border Accent -->
                        <div class="absolute left-0 top-0 bottom-0 w-1.5" :class="getRiskLevelBorder(plan.riskLevel)"></div>
                        
                        <div class="flex justify-between items-start mb-4 pl-3">
                           <div class="flex flex-col gap-2">
                              <h4 class="font-bold text-[16px] text-[var(--color-text-primary)] flex items-center group-hover:text-[var(--color-accent)] transition-colors">
                                 <i class="fa-solid fa-shield-halved text-[var(--color-text-muted)] mr-2 text-sm group-hover:text-[var(--color-accent)] transition-colors"></i>
                                 {{ plan.name }}
                              </h4>
                              <div class="flex flex-wrap items-center gap-2 mt-1">
                                 <span class="text-[10px] px-2.5 py-1 rounded-md font-bold tracking-wide uppercase border shadow-sm" :class="getRiskLevelClass(plan.riskLevel)">{{ plan.riskLevel }}</span>
                                 <span class="text-[10px] px-2.5 py-1 rounded-md font-bold tracking-wide uppercase border shadow-sm" :class="getRiskStatusClass(plan.riskStatus)">{{ plan.riskStatus }}</span>
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

                        <div class="bg-[var(--bg-secondary)] rounded-lg p-3.5 mb-4 border border-[var(--border-color)]/60 ml-3">
                           <div class="text-[11px] font-bold text-[var(--color-text-muted)] uppercase tracking-wider mb-1.5 flex items-center"><i class="fa-solid fa-bolt mr-1.5 text-[var(--color-warning)]"></i> Trigger (Điều kiện)</div>
                           <div class="text-[13px] text-[var(--color-text-primary)] font-medium leading-relaxed">
                              {{ plan.activationCondition }}
                           </div>
                        </div>

                        <div class="bg-[var(--color-accent)]/5 rounded-lg p-3.5 mb-5 border border-[var(--color-accent)]/20 ml-3" v-if="plan.contingencyTask">
                           <div class="text-[11px] font-bold text-[var(--color-accent)] uppercase tracking-wider mb-2 flex items-center"><i class="fa-solid fa-link mr-1.5"></i> Task dự phòng</div> 
                           <div class="cursor-pointer hover:underline text-[var(--color-text-primary)] font-semibold flex items-start text-[14px]" @click="openTaskDetail(plan.contingencyTask)">
                              <i class="fa-solid fa-square-check mt-0.5 mr-2 text-[var(--color-accent)] text-base"></i>
                              <span>{{ plan.contingencyTask.title }}</span>
                           </div>
                        </div>

                        <div class="flex justify-end items-center mt-3 pt-4 border-t border-[var(--border-color)] gap-3 pl-3" v-if="!plan.isActivated">
                           <button class="text-[13px] font-bold px-5 py-2 rounded-lg text-[var(--color-text-primary)] hover:bg-[var(--bg-secondary)] transition-all flex items-center" @click="openContingencyDetail(plan)">
                              <i class="fa-regular fa-file-lines mr-1.5 text-lg opacity-70"></i> Chi tiết
                           </button>
                           <button class="text-[13px] font-bold px-5 py-2 rounded-lg bg-[var(--color-accent)] text-white hover:opacity-90 hover:shadow-lg transition-all flex items-center active:scale-95 shadow-md" @click="confirmActivateContingencyPlan(plan)">
                              <i class="fa-solid fa-rocket mr-2 text-lg"></i> Kích hoạt
                           </button>
                        </div>
                        
                        <div v-else-if="plan.activatedBy" class="mt-3 pt-4 border-t border-[var(--color-success)]/20 flex flex-col sm:flex-row justify-between items-center sm:items-start gap-3 bg-green-50/50 -mr-5 -mb-5 ml-0 p-4 rounded-b-xl relative z-10" style="margin-left: 12px; margin-right: 0px;">
                           <div class="flex items-center gap-2">
                              <div class="w-8 h-8 rounded-full bg-green-100 text-green-600 flex items-center justify-center shadow-sm">
                                <i class="fa-solid fa-check text-lg"></i>
                              </div>
                              <div>
                                 <div class="text-sm font-bold text-green-700">Đã kích hoạt</div>
                                 <div class="text-[11px] text-[var(--color-text-muted)] mt-0.5">Lúc {{ new Date(plan.activatedAt).toLocaleString() }}</div>
                              </div>
                           </div>
                           <div class="text-right flex flex-col items-end">
                              <div class="text-xs text-[var(--color-text-muted)] font-medium">Người thao tác</div>
                              <div class="text-sm font-bold text-[var(--color-text-primary)]">{{ plan.activatedBy.fullName || plan.activatedBy.email }}</div>
                              <div v-if="plan.contingencyTask" class="mt-1.5 flex items-center gap-1.5">
                                 <span class="text-[10px] uppercase font-bold text-[var(--color-text-muted)] tracking-wider">Status:</span> 
                                 <span class="font-bold text-[11px] px-2 py-0.5 rounded shadow-sm bg-white border border-[var(--border-color)]" :style="{color: getStatusColor(plan.contingencyTask.statusName)}">{{ plan.contingencyTask.statusName }}</span>
                              </div>
                           </div>
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
            </div>\n`;
    content = content.slice(0, nextNextDivIdx + 6) + '\n' + contingencyHtml + content.slice(nextNextDivIdx + 6);
}

// 3. Inject dialogs
const dialogsHtml = `
<el-dialog v-model="showContingencyForm" :title="editingContingencyPlanId ? 'Cập nhật Kế hoạch dự phòng' : 'Thêm Kế hoạch dự phòng mới'" width="600px" class="theme-dialog" :teleported="false">
   <div class="cm-form-group">
      <label class="cm-label">Tên Kế hoạch <span class="text-red-500">*</span></label>
      <input type="text" class="cm-inputbox" v-model="contingencyPlanForm.name" placeholder="Ví dụ: Kế hoạch fallback máy chủ" />
   </div>
   <div class="grid grid-cols-2 gap-4">
      <div class="cm-form-group">
         <label class="cm-label">Mức độ rủi ro</label>
         <el-select v-model="contingencyPlanForm.riskLevel" class="w-full" :teleported="false">
            <el-option label="Critical (Nghiêm trọng)" value="Critical" />
            <el-option label="High (Cao)" value="High" />
            <el-option label="Medium (Trung bình)" value="Medium" />
            <el-option label="Low (Thấp)" value="Low" />
         </el-select>
      </div>
      <div class="cm-form-group">
         <label class="cm-label">Trạng thái rủi ro</label>
         <el-select v-model="contingencyPlanForm.riskStatus" class="w-full" :teleported="false">
            <el-option label="Safe (An toàn)" value="Safe" />
            <el-option label="Warning (Cảnh báo)" value="Warning" />
            <el-option label="At Risk (Rủi ro)" value="At Risk" />
            <el-option label="Critical (Nghiêm trọng)" value="Critical" />
         </el-select>
      </div>
   </div>
   <div class="cm-form-group mt-3">
      <label class="cm-label">Điều kiện kích hoạt (Trigger) <span class="text-red-500">*</span></label>
      <textarea class="cm-textareabox h-[80px]" v-model="contingencyPlanForm.activationCondition" placeholder="Ví dụ: Khi server chính vượt quá 95% CPU trong 10 phút..."></textarea>
   </div>
   <div class="cm-form-group mt-3">
      <label class="cm-label">Mô tả rủi ro (Risk Description)</label>
      <textarea class="cm-textareabox h-[80px]" v-model="contingencyPlanForm.riskDescription" placeholder="Mô tả chi tiết nguyên nhân rủi ro tiềm ẩn..."></textarea>
   </div>
   <div class="cm-form-group mt-3">
      <label class="cm-label">Task dự phòng <span class="text-red-500">*</span></label>
      <el-select v-model="contingencyPlanForm.contingencyTaskId" class="w-full" placeholder="Chọn một task dự phòng..." filterable clearable :teleported="false">
         <el-option v-for="t in allProjectTasks" :key="t.id" :label="t.title" :value="t.id">
            <div class="flex items-center">
               <i class="fa-solid fa-square-check mr-2 text-[var(--color-accent)]"></i>
               <span>{{ t.title }}</span>
            </div>
         </el-option>
      </el-select>
      <div class="text-xs text-muted mt-1">Một task có sẵn trong dự án sẽ đóng vai trò là giải pháp fallback. (Giao diện tạo task dự phòng mới đang được phát triển).</div>
   </div>
   <div class="cm-form-group mt-3">
      <label class="cm-label">Ghi chú & Phương án (Rich text)</label>
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

<el-drawer v-model="showContingencyDetail" size="500px" title="Chi tiết Kế hoạch dự phòng" class="theme-drawer" :teleported="false">
   <div v-if="viewingContingencyPlan" class="p-5 flex flex-col gap-6">
      <div>
         <h2 class="text-xl font-bold text-[var(--color-text-primary)] mb-2">{{ viewingContingencyPlan.name }}</h2>
         <div class="flex flex-wrap gap-2">
            <span class="text-[11px] px-3 py-1 rounded-full font-bold uppercase tracking-wider border shadow-sm" :class="getRiskLevelClass(viewingContingencyPlan.riskLevel)">Risk: {{ viewingContingencyPlan.riskLevel }}</span>
            <span class="text-[11px] px-3 py-1 rounded-full font-bold uppercase tracking-wider border shadow-sm" :class="getRiskStatusClass(viewingContingencyPlan.riskStatus)">Status: {{ viewingContingencyPlan.riskStatus }}</span>
         </div>
      </div>
      
      <div class="bg-[var(--bg-secondary)] rounded-xl p-4 border border-[var(--border-color)]">
         <div class="text-[12px] font-bold text-[var(--color-warning)] uppercase tracking-wider mb-2 flex items-center"><i class="fa-solid fa-bolt mr-2"></i> Điều kiện kích hoạt</div>
         <p class="text-[14px] text-[var(--color-text-primary)] leading-relaxed">{{ viewingContingencyPlan.activationCondition }}</p>
      </div>

      <div class="bg-[var(--bg-secondary)] rounded-xl p-4 border border-[var(--border-color)]" v-if="viewingContingencyPlan.riskDescription">
         <div class="text-[12px] font-bold text-[var(--color-danger)] uppercase tracking-wider mb-2 flex items-center"><i class="fa-solid fa-triangle-exclamation mr-2"></i> Mô tả rủi ro</div>
         <p class="text-[14px] text-[var(--color-text-primary)] leading-relaxed">{{ viewingContingencyPlan.riskDescription }}</p>
      </div>

      <div class="bg-[var(--color-accent)]/5 rounded-xl p-4 border border-[var(--color-accent)]/20">
         <div class="text-[12px] font-bold text-[var(--color-accent)] uppercase tracking-wider mb-2 flex items-center"><i class="fa-solid fa-link mr-2"></i> Task dự phòng liên kết</div>
         <div v-if="viewingContingencyPlan.contingencyTask" class="flex items-center justify-between p-3 bg-white dark:bg-gray-800 rounded-lg border border-[var(--border-color)] cursor-pointer hover:border-[var(--color-accent)] transition-colors" @click="openTaskDetail(viewingContingencyPlan.contingencyTask)">
            <div class="flex items-center gap-3 overflow-hidden">
               <i class="fa-solid fa-square-check text-xl text-[var(--color-accent)]"></i>
               <span class="font-semibold text-sm truncate">{{ viewingContingencyPlan.contingencyTask.title }}</span>
            </div>
            <i class="fa-solid fa-arrow-right text-[var(--color-text-muted)]"></i>
         </div>
      </div>

      <div v-if="viewingContingencyPlan.notes">
         <div class="text-[13px] font-bold text-[var(--color-text-primary)] uppercase tracking-wider mb-3">Ghi chú & Hướng dẫn</div>
         <div class="prose prose-sm dark:prose-invert max-w-none text-[14px] leading-relaxed p-4 bg-[var(--bg-secondary)] rounded-xl border border-[var(--border-color)]" v-html="viewingContingencyPlan.notes"></div>
      </div>
   </div>
</el-drawer>
`;
content = content.replace('</transition>\n</template>', dialogsHtml + '\n</transition>\n</template>');
content = content.replace('</transition>\r\n</template>', dialogsHtml + '\n</transition>\n</template>');


// 4. Inject JS logic
const jsLogic = `
// Contingency Plan logic
const contingencyPlans = ref([]);
const showContingencyForm = ref(false);
const editingContingencyPlanId = ref(null);
const contingencyPlanForm = ref({ name: '', riskLevel: 'Low', riskStatus: 'Safe', activationCondition: '', riskDescription: '', notes: '', contingencyTaskId: null });
const recoveryPlanEditor = ref(null);
const isSavingContingency = ref(false);
const isLoadingContingency = ref(false);

const showContingencyDetail = ref(false);
const viewingContingencyPlan = ref(null);

const handleRecoveryPlanInput = () => {
  if (recoveryPlanEditor.value) {
    contingencyPlanForm.value.notes = recoveryPlanEditor.value.innerHTML;
  }
};

const getRiskLevelClass = (level) => {
  switch (level) {
    case 'Critical': return 'border-red-200 bg-red-50 text-red-700';
    case 'High': return 'border-orange-200 bg-orange-50 text-orange-700';
    case 'Medium': return 'border-yellow-200 bg-yellow-50 text-yellow-700';
    default: return 'border-blue-200 bg-blue-50 text-blue-700';
  }
};
const getRiskLevelBorder = (level) => {
  switch (level) {
    case 'Critical': return 'bg-red-500';
    case 'High': return 'bg-orange-500';
    case 'Medium': return 'bg-yellow-500';
    default: return 'bg-blue-500';
  }
};
const getRiskStatusClass = (status) => {
  switch (status) {
    case 'Critical': return 'border-red-300 bg-red-100 text-red-800 shadow-[0_0_8px_rgba(239,68,68,0.3)]';
    case 'At Risk': return 'border-orange-300 bg-orange-100 text-orange-800';
    case 'Warning': return 'border-yellow-300 bg-yellow-100 text-yellow-800';
    default: return 'border-green-300 bg-green-100 text-green-800';
  }
};

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
  contingencyPlanForm.value = { name: '', riskLevel: 'Low', riskStatus: 'Safe', activationCondition: '', riskDescription: '', notes: '', contingencyTaskId: null };
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
  if (!contingencyPlanForm.value.name || !contingencyPlanForm.value.activationCondition || !contingencyPlanForm.value.contingencyTaskId) {
     ElMessage.warning('Vui lòng điền các trường bắt buộc (Tên, Điều kiện, Task dự phòng).');
     return;
  }
  isSavingContingency.value = true;
  try {
    const payload = {
       name: contingencyPlanForm.value.name,
       riskLevel: contingencyPlanForm.value.riskLevel,
       riskStatus: contingencyPlanForm.value.riskStatus,
       activationCondition: contingencyPlanForm.value.activationCondition,
       riskDescription: contingencyPlanForm.value.riskDescription,
       notes: contingencyPlanForm.value.notes,
       contingencyTaskId: contingencyPlanForm.value.contingencyTaskId
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
         riskStatus: plan.riskStatus, 
         activationCondition: plan.activationCondition,
         riskDescription: plan.riskDescription || '',
         notes: plan.notes,
         contingencyTaskId: plan.contingencyTaskId
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

function confirmActivateContingencyPlan(plan) {
   const taskTitle = plan.contingencyTask?.title || 'Unknown Task';
   ElMessageBox.confirm(
      \`<div class="mb-2">Bạn muốn kích hoạt kế hoạch dự phòng này?</div><div class="p-2 bg-gray-100 rounded text-sm mb-2"><b>Task dự phòng:</b> \${taskTitle}</div><div class="text-xs text-gray-500">Sau khi kích hoạt, Task dự phòng sẽ chuyển sang trạng thái hoạt động (In Progress).</div>\`,
      'Xác nhận kích hoạt',
      {
         confirmButtonText: 'Kích hoạt',
         cancelButtonText: 'Hủy',
         dangerouslyUseHTMLString: true,
         type: 'warning'
      }
   ).then(async () => {
      try {
         await axiosClient.post(\`/worktasks/\${props.selectedTask.id}/contingency-plans/\${plan.id}/activate\`);
         ElMessage.success('Kế hoạch đã được kích hoạt thành công');
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

// Comments logic
`;
content = content.replace('// Comments logic', jsLogic);

// 5. Inject CSS
const cssLogic = `
<style>
/* FORCE EL-DIALOG & EL-DRAWER Z-INDEX TO BE ON TOP OF TASK MODAL */
body .el-overlay {
    z-index: 99999 !important;
}
body .el-message-box__wrapper {
    z-index: 999999 !important;
}
.contingency-plans-container .contingency-card {
    padding: 20px !important;
    border-radius: 12px !important;
    border: 1px solid var(--border-color);
    margin-bottom: 16px;
    background: var(--bg-primary);
    box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06);
}
.contingency-plans-container .contingency-card-header {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    margin-bottom: 16px;
}
.contingency-plans-container .contingency-trigger-box {
    background: var(--bg-secondary);
    padding: 12px;
    border-radius: 8px;
    margin-bottom: 16px;
    border: 1px solid var(--border-color);
}
.contingency-plans-container .contingency-task-box {
    background: color-mix(in srgb, var(--color-accent) 5%, transparent);
    padding: 12px;
    border-radius: 8px;
    margin-bottom: 16px;
    border: 1px solid color-mix(in srgb, var(--color-accent) 20%, transparent);
}
.contingency-plans-container .contingency-footer {
    display: flex;
    justify-content: flex-end;
    gap: 12px;
    padding-top: 16px;
    border-top: 1px solid var(--border-color);
}
.c-badge {
    padding: 4px 10px;
    border-radius: 6px;
    font-size: 10px;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 0.5px;
    box-shadow: 0 1px 2px rgba(0,0,0,0.05);
}
</style>
`;
content = content + cssLogic;

// 6. Reset refs
content = content.replace(
  'taskDependencies.value = [];\r\n      assignedLabels.value = [];',
  'taskDependencies.value = [];\n      contingencyPlans.value = [];\n      showContingencyForm.value = false;\n      contingencyPlanForm.value = { name: \'\', riskLevel: \'Low\', riskStatus: \'Safe\', activationCondition: \'\', riskDescription: \'\', notes: \'\', contingencyTaskId: null };\n      assignedLabels.value = [];'
);
content = content.replace(
  'taskDependencies.value = [];\n      assignedLabels.value = [];',
  'taskDependencies.value = [];\n      contingencyPlans.value = [];\n      showContingencyForm.value = false;\n      contingencyPlanForm.value = { name: \'\', riskLevel: \'Low\', riskStatus: \'Safe\', activationCondition: \'\', riskDescription: \'\', notes: \'\', contingencyTaskId: null };\n      assignedLabels.value = [];'
);


fs.writeFileSync('src/components/TaskDetailModal.vue', content, 'utf8');
console.log('Successfully injected completely clean code!');
