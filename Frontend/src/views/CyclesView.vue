<script setup>
import NexusLayout from '@/components/layout/NexusLayout.vue'
import CyclesTab from '@/components/CyclesTab.vue'
import { useRoute } from 'vue-router'

const route = useRoute()
const projectId = route.params.id || 'current-project-id' // Defaulting if not in nested route
</script>

<template>
  <NexusLayout>
    <div style="background-color: #0D0F11; height: 100vh; overflow-y: auto;">
      <CyclesTab :projectId="projectId" />
    </div>
  </NexusLayout>
</template>
