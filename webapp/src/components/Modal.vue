<!-- Future implementations could make use of slots -->
<template>
  <div class="modal-backdrop show"></div>
  <div class="modal d-block" @click="$emit('close')">
    <div class="modal-dialog">
      <div class="modal-content">
        <div class="modal-header">
          <h5 class="modal-title" id="archiveModalLabel">{{ title }}</h5>
          <button type="button" class="close" @click="$emit('close')" aria-label="Close">
            <span aria-hidden="true">&times;</span>
          </button>
        </div>
        <div class="modal-body">
          {{ body }}
        </div>
        <div class="modal-footer">
          <slot name="footer">
            <button type="button" class="btn btn-secondary" @click="$emit('close')">Cancel</button>
            <button type="button" class="btn btn-danger" @click="$emit('continue')">Continue</button>
          </slot>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue'
export default defineComponent({
  props: {
    title: String,
    body: String
  }
})
</script>
