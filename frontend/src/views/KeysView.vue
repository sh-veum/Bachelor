<script setup lang="ts">
import { Button } from '@/components/ui/button'
import { toTypedSchema } from '@vee-validate/zod'
import { useForm } from 'vee-validate'
import * as z from 'zod'
import axios from 'axios';

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

import { FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form'

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
import ThemeCollapsible from '@/components/ThemeCollapsible.vue'
import { ref } from 'vue';
// Need to replace with actual themes and endpoints
const themesPlaceholder = [
  {
    themeName: 'Aquaculture List',
    accessibleEndpoints: [
        "/api/aquaculturelist/fishhealth/species",
        "/api/aquaculturelist/fishhealth/licenseelist"
    ]
  },
  {
    themeName: 'Cod Spawning Ground',
    accessibleEndpoints: [
        "/api/aquaculturelist/fishhealth/species",
        "/api/aquaculturelist/fishhealth/licenseelist"
    ]
  },
  {
    themeName: 'Disease History',
    accessibleEndpoints: [
        "/api/aquaculturelist/fishhealth/species",
        "/api/aquaculturelist/fishhealth/licenseelist"
    ]
  },
  {
    themeName: 'Export Restrictions',
    accessibleEndpoints: [
        "/api/aquaculturelist/fishhealth/species",
        "/api/aquaculturelist/fishhealth/licenseelist"
    ]
  }
]



const keys = ref([]);


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
  createAccessKey(values.name, values.themes);
})

const createAccessKey = async (keyName: string, themes: string[]) => {
  console.log('Creating access key:', keyName, themes)
  
  try {
    const response = await axios.post('http://localhost:8088/api/key/create-accesskey', {
      keyName: keyName,
      themes: themesPlaceholder,
    });
    console.log('Access key created:',  response.data)
  } catch (error) {
    console.error('Error creating access key:', (error as any).response.data);
    // Handle error
  }
}

const deleteAccessKey = async (encryptedKey: string) => {
  try {
    await axios.post('http://localhost:8088/api/key/delete-accesskey', { EncryptedKey: encryptedKey });
    console.log('Access key deleted');
  } catch (error) {
    console.error('Error deleting access key:', (error as any).response.data);
    // Handle error
  }
};

</script>

<template>
  <div class="flex justify-between">
    <h1 class="text-3xl font-semibold mb-8 px-2">Your API keys</h1>
    <Dialog>
      <DialogTrigger as-child>
        <Button class="mr-4"> Create new key </Button>
      </DialogTrigger>
      <DialogContent class="sm:max-w-[425px]">
        <DialogHeader>
          <DialogTitle>Create new key</DialogTitle>
          <DialogDescription>
            Choose a name for the key and which themes the key can access.
          </DialogDescription>
        </DialogHeader>
        <form class="space-y-6" @submit.prevent="onSubmit">
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
                              ? themesPlaceholder.find((t) => t.themeName === values.themes?.[0])?.label
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
                            v-for="theme in themesPlaceholder"
                            :key="theme.themeName"
                            :value="theme.themeName"
                            @select="
                              () => {
                                const selectedThemes = values.themes?.includes(theme.themeName)
                                  ? values.themes.filter((t) => t !== theme.themeName)
                                  : [...(values.themes ?? []), theme.themeName]
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
                                  values.themes?.includes(theme.themeName) ? 'opacity-100' : 'opacity-0'
                                )
                              "
                            />
                            {{ theme.themeName }}
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
      <TableRow v-for="(key, index) in keys" :key="index">
        <TableCell>{{ key.name }}</TableCell>
        <TableCell>
          <ThemeCollapsible v-if="key.themes.length > 1" :apiKey="key" />
          <div v-else class="py-3 font-mono text-sm">
            {{ key.themes[0] }}
          </div>
        </TableCell>
        <TableCell class="text-center"
          ><Button variant="destructive" @click="deleteAccessKey(key.encryptedKey)"> Delete </Button></TableCell
        >
      </TableRow>
    </TableBody>
  </Table>
</template>
