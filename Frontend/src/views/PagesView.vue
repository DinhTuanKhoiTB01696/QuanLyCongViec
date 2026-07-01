<script setup>

import PagesTab from '@/components/PagesTab.vue'
import { useRoute } from 'vue-router'

const route = useRoute()
const projectId = route.params.id || 'current-project-id'
</script>

<template>
  <PagesTab :projectId="projectId" />
</template>



