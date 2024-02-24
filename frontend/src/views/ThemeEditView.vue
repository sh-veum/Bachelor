<script setup lang="ts">
import CreateThemeDialog from '@/components/theme-view/CreateThemeDialog.vue'
import ThemeComponent from '@/components/theme-view/ThemeComponent.vue'
import { Button } from '@/components/ui/button'

// const themes = [
//   { value: 'AquaCultureLists', label: 'Aquaculture Lists' },
//   { value: 'CodSpawningGround', label: 'Cod Spawning Ground' },
//   { value: 'DiseaseHistory', label: 'Disease History' },
//   { value: 'Export Restrictions', label: 'Export Restrictions' }
// ]

const themes = [
  {
    name: 'AquaCultureLists',
    endpoints: ['/v1/geodata/fishhealth/licenseelist', '/v1/geodata/fishhealth/species']
  },
  { name: 'CodSpawningGround', endpoints: ['/v1/geodata/codspawningground/{id}'] },
  {
    name: 'DiseaseHistory',
    endpoints: ['/v1/geodata/fishhealth/locality/diseasezonehistory/{localityNo}/{year}/{week}']
  },
  {
    name: 'Export Restrictions',
    endpoints: [
      '/v1/geodata/fishhealth/exportrestrictions/{year}/{week}',
      '/v1/geodata/fishhealth/exportrestrictions/{localityNo}/{year}/{week}'
    ]
  }
]
</script>

<template>
  <CreateThemeDialog>
    <div class="flex">
      <Button class="ml-auto mr-4">Create Theme</Button>
    </div>
  </CreateThemeDialog>

  <div class="flex justify-center">
    <div class="flex-row w-1/2">
      <ThemeComponent v-for="theme in themes" :theme="theme" class="py-4" />
    </div>
  </div>
</template>
