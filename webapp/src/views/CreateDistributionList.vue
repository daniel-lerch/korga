<template>
    <div class="px-4 md:px-8 max-w-3xl mx-auto">
        <h1 class="text-2xl font-bold mb-4">Neue Verteilerliste</h1>

        <div class="bg-white shadow-sm rounded-md p-4">
            <div class="mb-4">
                <label class="block text-sm font-medium text-gray-700 mb-1">Alias</label>
                <input v-model="alias" type="text"
                    class="w-full border rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                    placeholder="z. B. Vorstand" />
            </div>

            <div class="mb-4 flex items-center gap-3">
                <input id="newsletter" v-model="newsletter" type="checkbox" class="h-4 w-4 text-blue-600" />
                <label for="newsletter" class="text-sm text-gray-700">Newsletter</label>
            </div>

            <div class="mb-4">
                <label class="block text-sm font-medium text-gray-700 mb-1">Filter (JSON / query)</label>
                <textarea v-model="recipientsQuery" rows="8"
                    class="w-full border rounded px-3 py-2 font-mono text-sm text-gray-800"
                    placeholder='Gib den Filterausdruck oder JSON hier ein'></textarea>
                <p class="text-xs text-gray-500 mt-2">Der Filter ist derzeit ein freier Textbereich. Eine bessere UI
                    folgt später.</p>
            </div>

            <div class="flex items-center gap-3">
                <button @click="onCancel" type="button"
                    class="px-4 py-2 rounded border bg-white text-gray-700 hover:bg-gray-50">Abbrechen</button>
                <button @click="onSubmit" :disabled="loading" type="button"
                    class="px-4 py-2 rounded bg-blue-600 text-white disabled:opacity-50">
                    <span v-if="!loading">Erstellen</span>
                    <span v-else>Erstelle…</span>
                </button>
            </div>

            <p v-if="error" class="mt-3 text-sm text-red-600">{{ error }}</p>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref } from "vue"
import { useRouter } from "vue-router"
import { createDistributionList } from "@/services/distribution-list"

const router = useRouter()

const alias = ref("")
const newsletter = ref(false)
const recipientsQuery = ref("")
const loading = ref(false)
const error = ref<string | null>(null)

function onCancel() {
    router.back()
}

async function onSubmit() {
    error.value = null
    if (!alias.value.trim()) {
        error.value = "Alias ist erforderlich"
        return
    }

    loading.value = true
    try {
        await createDistributionList({
            alias: alias.value.trim(),
            newsletter: newsletter.value,
            recipientsQuery: JSON.parse(recipientsQuery.value),
        })
        router.push({ name: "DistributionLists" })
    } catch (e) {
        error.value = e instanceof Error ? e.message : String(e)
    } finally {
        loading.value = false
    }
}
</script>
