<script setup>
import NexusLayout from '@/components/layout/NexusLayout.vue'
import PagesTab from '@/components/PagesTab.vue'
import { useRoute } from 'vue-router'

const route = useRoute()
const projectId = route.params.id || 'current-project-id'
</script>

<template>
  <NexusLayout>
    <div style="background-color: #0D0F11; height: 100vh; overflow-y: auto;">
      <PagesTab :projectId="projectId" />
    </div>
  </NexusLayout>
</template>
