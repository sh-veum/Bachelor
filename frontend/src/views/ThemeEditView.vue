<script setup lang="ts">
import { Button } from '@/components/ui/button'
import { toTypedSchema } from '@vee-validate/zod'
import { useForm } from 'vee-validate'
import * as z from 'zod'

import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger
} from '@/components/ui/dialog'
import { Input } from '@/components/ui/input'

import { FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form'

import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList
} from '@/components/ui/command'
import { Popover, PopoverContent, PopoverTrigger } from '@/components/ui/popover'
import { cn } from '@/lib/utils'
import { Check, ChevronsUpDown } from 'lucide-vue-next'

// const themes = [
//   { value: 'AquaCultureLists', label: 'Aquaculture Lists' },
//   { value: 'CodSpawningGround', label: 'Cod Spawning Ground' },
//   { value: 'DiseaseHistory', label: 'Disease History' },
//   { value: 'Export Restrictions', label: 'Export Restrictions' }
// ]

// const themes = [
//   {
//     name: 'AquaCultureLists',
//     endpoints: ['/v1/geodata/fishhealth/licenseelist', '/v1/geodata/fishhealth/species']
//   },
//   { name: 'CodSpawningGround', endpoints: ['/v1/geodata/codspawningground/{id}'] },
//   {
//     name: 'DiseaseHistory',
//     endpoints: ['/v1/geodata/fishhealth/locality/diseasezonehistory/{localityNo}/{year}/{week}']
//   },
//   {
//     name: 'Export Restrictions',
//     endpoints: [
//       '/v1/geodata/fishhealth/exportrestrictions/{year}/{week}',
//       '/v1/geodata/fishhealth/exportrestrictions/{localityNo}/{year}/{week}'
//     ]
//   }
// ]

const endpoints = [
  '/v1/geodata/fishhealth/licenseelist',
  '/v1/geodata/fishhealth/species',
  '/v1/geodata/codspawningground/{id}',
  '/v1/geodata/fishhealth/locality/diseasezonehistory/{localityNo}/{year}/{week}',
  '/v1/geodata/fishhealth/exportrestrictions/{year}/{week}',
  '/v1/geodata/fishhealth/exportrestrictions/{localityNo}/{year}/{week}'
]

const formSchema = toTypedSchema(
  z.object({
    name: z
      .string({
        required_error: 'Please enter a name.'
      })
      .min(1, 'Please enter a name.'),
    endpoints: z.array(z.string()).min(1, 'Please select at least one endpoint.')
  })
)

const { handleSubmit, setValues, values } = useForm({
  validationSchema: formSchema
})

const onSubmit = handleSubmit((values) => {
  console.log(values)
})
</script>

<template>
  <Dialog>
    <DialogTrigger as-child>
      <Button class="ml-auto mr-4"> Create new theme </Button>
    </DialogTrigger>
    <DialogContent class="sm:max-w-[425px]">
      <DialogHeader>
        <DialogTitle>Create new theme</DialogTitle>
        <DialogDescription>
          Choose a name for the theme and which endpoints are included in the theme.
        </DialogDescription>
      </DialogHeader>
      <form class="space-y-6" @submit="onSubmit">
        <div class="grid gap-4 py-4">
          <FormField v-slot="{ componentField }" name="name">
            <FormItem>
              <FormLabel>Name</FormLabel>
              <FormControl>
                <Input type="text" placeholder="Name" v-bind="componentField" />
              </FormControl>
              <FormMessage />
            </FormItem>
          </FormField>

          <FormField name="endpoints">
            <FormItem class="flex flex-col">
              <FormLabel>Endpoints</FormLabel>
              <Popover>
                <PopoverTrigger as-child>
                  <FormControl>
                    <Button
                      variant="outline"
                      role="combobox"
                      :class="
                        cn(
                          'w-auto justify-between',
                          !values.endpoints?.length && 'text-muted-foreground'
                        )
                      "
                    >
                      {{
                        values.endpoints?.length
                          ? values.endpoints.length === 1
                            ? endpoints.find((e) => e === values.endpoints?.[0])
                            : `${values.endpoints.length} endpoints selected`
                          : 'Select endpoints...'
                      }}
                      <ChevronsUpDown class="ml-2 h-4 w-4 shrink-0 opacity-50" />
                    </Button>
                  </FormControl>
                </PopoverTrigger>
                <PopoverContent class="w-auto p-0">
                  <Command multiple>
                    <CommandInput placeholder="Search endpoints..." />
                    <CommandEmpty>Nothing found.</CommandEmpty>
                    <CommandList>
                      <CommandGroup>
                        <CommandItem
                          v-for="endpoint in endpoints"
                          :key="endpoint"
                          :value="endpoint"
                          @select="
                            () => {
                              const selectedEndpoints = values.endpoints?.includes(endpoint)
                                ? values.endpoints.filter((e) => e !== endpoint)
                                : [...(values.endpoints ?? []), endpoint]
                              setValues({
                                endpoints: selectedEndpoints
                              })
                            }
                          "
                        >
                          <Check
                            :class="
                              cn(
                                'mr-2 h-4 w-4',
                                values.endpoints?.includes(endpoint) ? 'opacity-100' : 'opacity-0'
                              )
                            "
                          />
                          {{ endpoint }}
                        </CommandItem>
                      </CommandGroup>
                    </CommandList>
                  </Command>
                </PopoverContent>
              </Popover>
              <FormMessage />
            </FormItem>
          </FormField>
        </div>
        <DialogFooter>
          <Button type="submit"> Create </Button>
        </DialogFooter>
      </form>
    </DialogContent>
  </Dialog>
</template>
