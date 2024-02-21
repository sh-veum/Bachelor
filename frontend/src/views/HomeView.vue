<script setup lang="ts">
import ThemeCollapsible from '@/components/ThemeCollapsible.vue'
import { Button } from '@/components/ui/button'
import { toTypedSchema } from '@vee-validate/zod'
import { useForm } from 'vee-validate'
import * as z from 'zod'

import {
  Table,
  TableBody,
  TableCaption,
  TableCell,
  TableHead,
  TableHeader,
  TableRow
} from '@/components/ui/table'

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

import {
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage
} from '@/components/ui/form'

import { Check, ChevronsUpDown } from 'lucide-vue-next'

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

const themes = [
  { value: 'AquaCultureLists', label: 'Aquaculture Lists' },
  { value: 'CodSpawningGround', label: 'Cod Spawning Ground' },
  { value: 'DiseaseHistory', label: 'Disease History' },
  { value: 'Export Restrictions', label: 'Export Restrictions' }
]

const keys = [
  { name: 'Key 1', themes: ['Theme 1', 'Theme 2', 'Theme 3'] },
  { name: 'Key 2', themes: ['Theme 1'] },
  {
    name: 'Key 3 has a very long name for sure',
    themes: ['Theme 1', 'Theme 2', 'Theme 3']
  }
]

const formSchema = toTypedSchema(
  z.object({
    name: z
      .string({
        required_error: 'Please enter a name.'
      })
      .min(1, 'Please enter a name.'),
    themes: z.array(z.string()).min(1, 'Please select at least one theme.')
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
  <div class="flex">
    <h1 class="text-3xl font-semibold mb-8 px-2">Your API keys</h1>
    <Dialog>
      <DialogTrigger as-child>
        <Button class="ml-auto mr-4"> Create new key </Button>
      </DialogTrigger>
      <DialogContent class="sm:max-w-[425px]">
        <DialogHeader>
          <DialogTitle>Create new key</DialogTitle>
          <DialogDescription>
            Choose a name for the key and which themes the key can access.
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

            <FormField name="themes">
              <FormItem class="flex flex-col">
                <FormLabel>Themes</FormLabel>
                <Popover>
                  <PopoverTrigger as-child>
                    <FormControl>
                      <Button
                        variant="outline"
                        role="combobox"
                        :class="
                          cn(
                            'w-[200px] justify-between',
                            !values.themes?.length && 'text-muted-foreground'
                          )
                        "
                      >
                        {{
                          values.themes?.length
                            ? values.themes.length === 1
                              ? themes.find((t) => t.value === values.themes?.[0])?.label
                              : `${values.themes.length} themes selected`
                            : 'Select themes...'
                        }}
                        <ChevronsUpDown class="ml-2 h-4 w-4 shrink-0 opacity-50" />
                      </Button>
                    </FormControl>
                  </PopoverTrigger>
                  <PopoverContent class="w-[200px] p-0">
                    <Command multiple>
                      <CommandInput placeholder="Search themes..." />
                      <CommandEmpty>Nothing found.</CommandEmpty>
                      <CommandList>
                        <CommandGroup>
                          <CommandItem
                            v-for="theme in themes"
                            :key="theme.value"
                            :value="theme.value"
                            @select="
                              () => {
                                const selectedThemes = values.themes?.includes(theme.value)
                                  ? values.themes.filter((t) => t !== theme.value)
                                  : [...(values.themes ?? []), theme.value]
                                setValues({
                                  themes: selectedThemes
                                })
                              }
                            "
                          >
                            <Check
                              :class="
                                cn(
                                  'mr-2 h-4 w-4',
                                  values.themes?.includes(theme.value) ? 'opacity-100' : 'opacity-0'
                                )
                              "
                            />
                            {{ theme.label }}
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
  </div>

  <Table>
    <TableCaption>A list of your created keys</TableCaption>
    <TableHeader>
      <TableRow>
        <TableHead class="w-[200px]">Name</TableHead>
        <TableHead>Themes</TableHead>
        <TableHead class="w-[200px] text-center">Actions</TableHead>
      </TableRow>
    </TableHeader>
    <TableBody>
      <TableRow v-for="key in keys">
        <TableCell>{{ key.name }}</TableCell>
        <TableCell>
          <ThemeCollapsible v-if="key.themes.length > 1" :msg="'wtf'" />
          <div v-else class="py-3 font-mono text-sm">
            {{ key.themes[0] }}
          </div>
        </TableCell>
        <TableCell class="text-center"
          ><Button variant="destructive" @click=""> Delete </Button></TableCell
        >
      </TableRow>
    </TableBody>
  </Table>
</template>
