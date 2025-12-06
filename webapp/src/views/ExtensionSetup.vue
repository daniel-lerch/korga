<template>
  <div class="m-2">
    <h2>Mailist Backend</h2>
    <Form v-slot="$form" :initialValues="{ backendUrl: extension.backendUrl }" :resolver="validate"
      :validateOnValueUpdate="false" :validateOnMount="true" @submit="save">
      <div class="flex flex-col gap-1 mb-4">
        <InputText name="backendUrl" type="text" placeholder="Backend URL" />
        <Message v-if="$form.backendUrl?.dirty" severity="secondary" size="small" variant="simple">
          Deine ChurchTools-Zugangsdaten werden an das Backend übertragen.
        </Message>
        <Message v-if="$form.backendUrl?.invalid" severity="error" size="small" variant="simple">
          {{ $form.backendUrl.error.message }}
        </Message>
      </div>
      <Button type="submit" label="Speichern" />
    </Form>
  </div>
  <!-- TODO: Add configuration for ChurchTools user, IMAP, and SMTP -->
</template>

<script setup lang="ts">
import Button from "primevue/button"
import InputText from "primevue/inputtext";
import Message from "primevue/message";
import { Form, type FormResolverOptions, type FormSubmitEvent } from "@primevue/forms"
import { useExtensionStore } from '@/stores/extension';

const extension = useExtensionStore()
if (extension.moduleId === 0) {
  await extension.load()
}

// ValidateOnMount triggers a validation when the component is loaded.
if (extension.accessToken === "") {
  try {
    await extension.login()
  } catch {
  }
}

async function validate({ values }: FormResolverOptions) {
  try {
    await extension.authenticate(values.backendUrl)
    return {
      values,
      errors: { backendUrl: [] }
    }
  } catch (error) {
    return { errors: { backendUrl: [error] } }
  }
}

async function save(e: FormSubmitEvent) {
  if (e.valid) {
    try {
      console.log("Saving backend URL", e)
      extension.setBackendUrl(e.values.backendUrl)
      extension.login(e.values.backendUrl)
      e.reset()
    } catch (error) {
      alert("Fehler bei der Verbindung: " + error)
    }
  }
}
</script>