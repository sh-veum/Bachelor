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
import { ref, onMounted, watch, watchEffect } from 'vue'
import axios from 'axios'

// TODO: use a shared interface for theme
const props = defineProps<{
  isOpen: boolean
  theme?: {
    id: string
    themeName: string
    accessibleEndpoints: string[]
    isDeprecated: boolean
  }
}>()

const emit = defineEmits(['submit'])

const endpoints = ref<string[]>([])

const fetchData = async () => {
  try {
    //TODO: find a way to make the url more dynamic?
    const endpointsResponse = await axios.get(
      'http://localhost:8088/api/database/get-default-endpoints'
    )
    endpoints.value = endpointsResponse.data.map((endpoint: any) => endpoint.path)
  } catch (error) {
    console.error('Failed to fetch data:', error)
  }
}

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

const createTheme = async (values: { name: string; endpoints: string[] }) => {
  try {
    await axios.post('http://localhost:8088/api/rest/create-theme', {
      themeName: values.name,
      accessibleEndpoints: values.endpoints,
      isDeprecated: false
    })
  } catch (error) {
    console.error('Failed to create theme:', error)
  }
}

const editTheme = async (id: string, values: { name: string; endpoints: string[] }) => {
  try {
    //TODO: change backend to be able to use `http://localhost:8088/api/key/update-theme?id=${id}`?
    await axios.put('http://localhost:8088/api/rest/update-theme', {
      id,
      themeName: values.name,
      accessibleEndpoints: values.endpoints,
      isDeprecated: props.theme?.isDeprecated
    })
  } catch (error) {
    console.error('Failed to edit theme:', error)
  }
}

const onSubmit = handleSubmit((values) => {
  if (props.theme) {
    editTheme(props.theme.id, values).then(() => emit('submit'))
  } else {
    createTheme(values).then(() => emit('submit'))
  }
})

// Set values when the dialog is opened
// To ensure that the form is filled out when the dialog is opened
watch(
  () => props.isOpen,
  (isOpen) => {
    if (isOpen) {
      setValues({
        name: props.theme?.themeName ?? '',
        endpoints: props.theme?.accessibleEndpoints ?? []
      })
    }
  }
)

onMounted(fetchData)
</script>

<template>
  <Dialog>
    <DialogTrigger as-child>
      <slot></slot>
    </DialogTrigger>
    <DialogContent class="sm:max-w-[425px]">
      <DialogHeader>
        <DialogTitle>{{ theme ? 'Edit' : 'Create new' }} theme</DialogTitle>
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
          <Button type="submit">{{ theme ? 'Save' : 'Create' }}</Button>
        </DialogFooter>
      </form>
    </DialogContent>
  </Dialog>
</template>
