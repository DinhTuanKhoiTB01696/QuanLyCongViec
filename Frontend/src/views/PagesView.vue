<script setup>

import PagesTab from '@/components/PagesTab.vue'
import { useRoute } from 'vue-router'

const route = useRoute()
const projectId = route.params.id || 'current-project-id'
</script>

<template>
  <div>
    <div style="background-color: var(--color-bg); height: 100vh; overflow-y: auto;">
      <PagesTab :projectId="projectId" />
    </div>
  </div>
</template>



