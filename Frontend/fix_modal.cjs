const fs = require('fs');
const path = require('path');
const file = 'd:\\A\\QuanLyCongViec\\Frontend\\src\\components\\TaskDetailModal.vue';
let content = fs.readFileSync(file, 'utf8');

// replace v-if="showTaskModal" with nothing
content = content.replace('v-if="showTaskModal"', '');
// replace showTaskModal = false with ('close')
content = content.replace(/showTaskModal = false/g, '\(\'close\')');

// add props and emits
const scriptStr = <script setup>
import { ref, computed } from 'vue'

const props = defineProps({
  selectedTask: { type: Object, default: () => ({}) },
  projectId: { type: String, required: true }
})

const emit = defineEmits(['close', 'updateTask'])

// Forward update tasks to parent
const updateTask = (task, field, value) => {
  emit('updateTask', task, field, value)
}
</script>
<style scoped>
/* Scoped styles can be added here if needed, most styles are likely global in SpaceSummary from before */
</style>
;

content = content.replace('<script setup>\n</script>', scriptStr);

fs.writeFileSync(file, content, 'utf8');
console.log('TaskDetailModal.vue updated');
