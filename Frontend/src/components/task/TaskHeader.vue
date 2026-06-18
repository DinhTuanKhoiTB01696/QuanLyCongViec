<template>
  <div class="sp-header">
    <div class="sph-left">
      <button v-if="canGoBack" class="nav-icon-btn" type="button" @click="$emit('back')">
        <ArrowLeft class="w-4 h-4" />
      </button>
      <ArrowRight class="w-4 h-4 icon-btn" @click="$emit('close')" />
    </div>
    <div class="sph-right">
      <SprintaButton variant="outline" @click="$emit('toggle-subscription')">
        <BellOff class="w-4 h-4" v-if="isSubscribed" />
        <Bell class="w-4 h-4" v-else />
        {{ isSubscribed ? 'Unsubscribe' : 'Subscribe' }}
      </SprintaButton>
      <SprintaButton variant="default" @click="$emit('copy-link')">
        <Link class="w-4 h-4" /> 
        Copy link
      </SprintaButton>
      <SprintaDropdown trigger="click" @command="(cmd) => $emit('menu-command', cmd)">
        <SprintaButton variant="default" class="px-2" title="More actions">
          <MoreHorizontal class="w-4 h-4" />
        </SprintaButton>
        <template #dropdown>
          <el-dropdown-menu class="theme-dropdown">
            <el-dropdown-item command="copy"><Link class="w-4 h-4 mr-2" /> Copy link</el-dropdown-item>
            <el-dropdown-item command="duplicate"><Copy class="w-4 h-4 mr-2" /> Duplicate</el-dropdown-item>
            <el-dropdown-item command="archive"><Archive class="w-4 h-4 mr-2" /> Archive soon</el-dropdown-item>
          </el-dropdown-menu>
        </template>
      </SprintaDropdown>
    </div>
  </div>
</template>

<script setup>
import { ArrowLeft, ArrowRight, Bell, BellOff, Link, MoreHorizontal, Copy, Archive } from 'lucide-vue-next'
import SprintaButton from '@/components/ui/SprintaButton.vue'
import SprintaDropdown from '@/components/ui/SprintaDropdown.vue'

defineProps({
  canGoBack: { type: Boolean, default: false },
  isSubscribed: { type: Boolean, default: false }
})

defineEmits(['back', 'close', 'toggle-subscription', 'copy-link', 'menu-command'])
</script>
