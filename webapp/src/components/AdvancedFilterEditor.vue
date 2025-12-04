<template>
  <div class="flex flex-col gap-1">
    <Textarea v-model="text" :invalid="invalid" rows="8" fluid />
    <Message v-if="invalid" severity="error" size="small" variant="simple">
      Ungültiges JSON. Bitte überprüfe die Syntax.
    </Message>
    <Message severity="secondary" size="small" variant="simple">
      Du kannst hier beliebig komplexe Filterausdrücke aus <a href="/churchquery">ChurchQuery</a> verwenden.
    </Message>
  </div>
</template>

<script setup lang="ts">
import Message from "primevue/message";
import Textarea from "primevue/textarea";
import { ref, watch } from "vue";

const props = defineProps({
  modelValue: {
    required: true,
  }
})

const emit = defineEmits<{
  (e: 'update:modelValue', value: unknown): void
}>()

const text = ref(JSON.stringify(props.modelValue))
const invalid = ref(false)

watch(text, (newValue) => {
  try {
    const value = JSON.parse(newValue)
    invalid.value = false
    emit('update:modelValue', value)
  } catch {
    invalid.value = true
  }
})
</script>