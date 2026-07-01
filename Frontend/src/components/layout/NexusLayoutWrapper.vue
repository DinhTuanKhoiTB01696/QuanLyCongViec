<template>
  <NexusLayout>
    <router-view v-slot="{ Component }">
      <Transition name="route-soft" mode="out-in">
        <component :is="Component" />
      </Transition>
    </router-view>
  </NexusLayout>
</template>

<script setup>
import NexusLayout from '@/components/layout/NexusLayout.vue'
</script>
