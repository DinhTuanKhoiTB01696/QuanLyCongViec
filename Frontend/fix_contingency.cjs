const fs = require('fs');
const path = require('path');
const file = 'c:/Users/tua46/OneDrive/Máy tính/DATN_2/QuanLyCongViec/QuanLyCongViec/Frontend/src/components/TaskDetailModal.vue';
let content = fs.readFileSync(file, 'utf8');

// The replacement JS logic
const newJsLogic = \// Contingency Plan logic
const contingencyPlans = ref([]);
const showContingencyForm = ref(false);
const editingContingencyPlanId = ref(null);
const contingencyPlanForm = ref({ name: '', riskLevel: 'Low', riskStatus: 'Safe', activationCondition: '', notes: '' });
const recoveryPlanEditor = ref(null);
const isSavingContingency = ref(false);
const isLoadingContingency = ref(false);

const handleRecoveryPlanInput = () => {
  if (recoveryPlanEditor.value) {
    contingencyPlanForm.value.notes = recoveryPlanEditor.value.innerHTML;
  }
};

const getRiskLevelClass = (level) => {
  switch (level) {
    case 'Critical': return 'bg-red-100 text-red-800 border border-red-200';
    case 'High': return 'bg-orange-100 text-orange-800 border border-orange-200';
    case 'Medium': return 'bg-yellow-100 text-yellow-800 border border-yellow-200';
    default: return 'bg-blue-100 text-blue-800 border border-blue-200';
  }
};

const getRiskStatusClass = (status) => {
  switch (status) {
    case 'Critical': return 'text-red-600 font-bold bg-red-100';
    case 'At Risk': return 'text-orange-600 font-semibold bg-orange-100';
    case 'Warning': return 'text-yellow-600 font-semibold bg-yellow-100';
    default: return 'text-green-600 font-semibold bg-green-100';
  }
};

async function fetchContingencyPlans() {
  if (!props.selectedTask || !props.selectedTask.id) return;
  isLoadingContingency.value = true;
  try {
    const res = await axiosClient.get(\\\/worktasks/\\\/contingency-plans\\\);
    if (res.data?.data) {
       contingencyPlans.value = res.data.data;
    }
  } catch (err) {
    console.error("Lỗi khi tải danh sách Contingency Plans:", err);
  } finally {
    isLoadingContingency.value = false;
  }
}

function openCreateContingencyForm() {
   editingContingencyPlanId.value = null;
   contingencyPlanForm.value = { name: '', riskLevel: 'Low', riskStatus: 'Safe', activationCondition: '', notes: '' };
   if (recoveryPlanEditor.value) recoveryPlanEditor.value.innerHTML = '';
   showContingencyForm.value = true;
}

function editContingencyPlan(plan) {
   editingContingencyPlanId.value = plan.id;
   contingencyPlanForm.value = { ...plan };
   showContingencyForm.value = true;
   setTimeout(() => {
     if (recoveryPlanEditor.value) recoveryPlanEditor.value.innerHTML = plan.notes || '';
   }, 50);
}

function cancelContingencyForm() {
   showContingencyForm.value = false;
}

async function saveContingencyPlan() {
  if (!contingencyPlanForm.value.name || !contingencyPlanForm.value.activationCondition) {
     ElMessage.warning('Vui lòng điền Tên kế hoạch và Điều kiện kích hoạt.');
     return;
  }
  isSavingContingency.value = true;
  try {
    if (recoveryPlanEditor.value) contingencyPlanForm.value.notes = recoveryPlanEditor.value.innerHTML;
    
    if (editingContingencyPlanId.value) {
       await axiosClient.put(\\\/worktasks/\\\/contingency-plans/\\\\\\, contingencyPlanForm.value);
       ElMessage.success('Cập nhật kế hoạch dự phòng thành công');
    } else {
       await axiosClient.post(\\\/worktasks/\\\/contingency-plans\\\, contingencyPlanForm.value);
       ElMessage.success('Thêm kế hoạch dự phòng thành công');
    }
    showContingencyForm.value = false;
    await fetchContingencyPlans();
    fetchAuditTimeline();
  } catch (err) {
    ElMessage.error(err.response?.data?.message || 'Lưu thất bại');
  } finally {
    isSavingContingency.value = false;
  }
}

function confirmDeleteContingencyPlan(planId) {
   ElMessageBox.confirm('Bạn có chắc chắn muốn xóa kế hoạch dự phòng này?', 'Xác nhận xóa', {
      confirmButtonText: 'Xóa',
      cancelButtonText: 'Hủy',
      type: 'warning'
   }).then(async () => {
      try {
         await axiosClient.delete(\\\/worktasks/\\\/contingency-plans/\\\\\\);
         ElMessage.success('Đã xóa kế hoạch dự phòng');
         await fetchContingencyPlans();
      } catch (err) {
         ElMessage.error('Xóa thất bại');
      }
   }).catch(() => {});
}

function activateContingencyPlan(planId) {
   ElMessageBox.confirm('Khi kích hoạt, hệ thống sẽ tạo một Task mới cho kế hoạch dự phòng này. Bạn có muốn tiếp tục?', 'Xác nhận kích hoạt', {
      confirmButtonText: 'Kích hoạt',
      cancelButtonText: 'Hủy',
      type: 'info'
   }).then(async () => {
      try {
         await axiosClient.post(\\\/worktasks/\\\/contingency-plans/\\\/activate\\\);
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
});\

const startIdx = content.indexOf('// Contingency Plan logic');
const endIdx = content.indexOf('// Comments logic');
if(startIdx !== -1 && endIdx !== -1) {
    content = content.substring(0, startIdx) + newJsLogic + '\n\n' + content.substring(endIdx);
}

// Revert the wrong replace tool change
content = content.replace('payload.totalEstimatedHours = suggestedHours;\n        }\n\n        await persistTaskPatch(task, payload);', 'estimatedHours: assignee.estimatedHours || 0\n            }))\n        };\n\n        if (!isParentDerived) {\n            payload.totalEstimatedHours = suggestedHours;\n        }\n\n        await persistTaskPatch(task, payload);');

fs.writeFileSync(file, content, 'utf8');
