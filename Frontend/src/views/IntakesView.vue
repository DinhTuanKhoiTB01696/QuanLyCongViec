<script setup>

import IntakeInbox from '@/components/IntakeInbox.vue'
import { useRoute } from 'vue-router'

const route = useRoute()
const projectId = route.params.id || localStorage.getItem('lastProjectId')
</script>

<template>
  <div>
    <div style="background-color: var(--color-bg); height: 100vh; overflow-y: auto; padding: 24px;">
      <IntakeInbox :projectId="projectId" />
    </div>
  </div>
</template>


