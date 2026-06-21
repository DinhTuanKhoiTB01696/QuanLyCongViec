<template>
  <header class="sprinta-detail-header" :class="{ 'with-cover': withCover, 'is-compact': !withCover }">
    <template v-if="withCover">
      <div 
        class="sprinta-header-cover" 
        :style="coverImageUrl ? { backgroundImage: `url(${coverImageUrl})` } : { backgroundColor: coverColor || '#0052CC' }"
      >
        <div class="sprinta-header-cover-actions">
          <slot name="cover-actions"></slot>
        </div>
      </div>
      <div class="sprinta-header-info-row">
        <div class="sprinta-header-title-group" style="align-items: flex-end;">
          <div class="sprinta-header-avatar" :style="{ backgroundColor: avatarColor || '#0052CC' }">
            {{ icon }}
          </div>
          <div style="padding-bottom: 8px;">
            <slot name="breadcrumbs"></slot>
            <h1 class="sprinta-header-title">{{ title }}</h1>
          </div>
        </div>
        <div class="sprinta-header-actions" style="padding-bottom: 8px;">
          <slot name="actions"></slot>
        </div>
      </div>
    </template>
    <template v-else>
      <div class="sprinta-header-content">
        <div class="sprinta-header-title-group">
          <div class="sprinta-header-icon" :style="{ backgroundColor: avatarColor || '#E6FCFF', color: avatarTextColor || '#00B8D9' }">
            {{ icon }}
          </div>
          <div>
            <slot name="breadcrumbs"></slot>
            <h1 class="sprinta-header-title">{{ title }}</h1>
          </div>
        </div>
        <div class="sprinta-header-actions">
          <slot name="actions"></slot>
        </div>
      </div>
    </template>
    
    <div class="sprinta-tabs-compact">
      <slot name="tabs"></slot>
    </div>
  </header>
</template>

<script setup>
defineProps({
  withCover: {
    type: Boolean,
    default: false
  },
  coverImageUrl: String,
  coverColor: String,
  title: {
    type: String,
    required: true
  },
  icon: {
    type: String,
    default: ''
  },
  avatarColor: String,
  avatarTextColor: String
})
</script>
