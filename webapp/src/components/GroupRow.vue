<template>
  <div class="row">
    <div class="col">{{ group.name }}</div>
    <div class="col">{{ group.description }}</div>
    <div class="col">{{ group.memberCount }}</div>
  </div>
</template>

<script lang="ts">
import { defineComponent, PropType } from "vue";
import { GroupResponse } from "../services/group";

export default defineComponent({
  props: {
    group: {
      type: Object as PropType<GroupResponse>,
      required: true,
    },
  },
});
</script>
