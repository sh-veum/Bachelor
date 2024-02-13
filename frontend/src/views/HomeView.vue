<script setup lang="ts">
import ThemeCollapsible from '@/components/ThemeCollapsible.vue'
import { Button } from '@/components/ui/button'

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

import { Check, ChevronsUpDown } from 'lucide-vue-next'
import { ref } from 'vue'

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

const frameworks = [
  { value: 'next.js', label: 'Next.js' },
  { value: 'sveltekit', label: 'SvelteKit' },
  { value: 'nuxt.js', label: 'Nuxt.js' },
  { value: 'remix', label: 'Remix' },
  { value: 'astro', label: 'Astro' }
]

const open = ref(false)
const value = ref<string>('')

// const filterFunction = (list: typeof frameworks, search: string) => list.filter(i => i.value.toLowerCase().includes(search.toLowerCase()))

const keys = [
  { name: 'Key 1', themes: ['Theme 1', 'Theme 2', 'Theme 3'] },
  { name: 'Key 2', themes: ['Theme 1', 'Theme 2', 'Theme 3'] },
  { name: 'Key 3', themes: ['Theme 1', 'Theme 2', 'Theme 3'] }
]
</script>

<template>
  <Dialog>
    <DialogTrigger as-child>
      <Button> Create new key </Button>
    </DialogTrigger>
    <DialogContent class="sm:max-w-[425px]">
      <DialogHeader>
        <DialogTitle>Create new key</DialogTitle>
        <DialogDescription>
          Choose a name for the key and which themes the key can access.
        </DialogDescription>
      </DialogHeader>
      <div class="grid gap-4 py-4">
        <div class="grid grid-cols-4 items-center gap-4">
          <!-- TODO: maybe have a label? -->
          <!-- <Label for="name" class="text-right">
            Name
          </Label> -->
          <Input id="name" class="col-span-3" placeholder="Name" />
        </div>
        <Popover v-model:open="open">
          <PopoverTrigger as-child>
            <Button
              variant="outline"
              role="combobox"
              :aria-expanded="open"
              class="w-[200px] justify-between"
            >
              {{
                value
                  ? frameworks.find((framework) => framework.value === value)?.label
                  : 'Select framework...'
              }}
              <ChevronsUpDown class="ml-2 h-4 w-4 shrink-0 opacity-50" />
            </Button>
          </PopoverTrigger>
          <PopoverContent class="w-[200px] p-0">
            <Command>
              <CommandInput class="h-9" placeholder="Search framework..." />
              <CommandEmpty>No framework found.</CommandEmpty>
              <CommandList>
                <CommandGroup>
                  <CommandItem
                    v-for="framework in frameworks"
                    :key="framework.value"
                    :value="framework.value"
                    @select="
                      (ev) => {
                        if (typeof ev.detail.value === 'string') {
                          value = ev.detail.value
                        }
                        open = false
                      }
                    "
                  >
                    {{ framework.label }}
                    <Check
                      :class="
                        cn(
                          'ml-auto h-4 w-4',
                          value === framework.value ? 'opacity-100' : 'opacity-0'
                        )
                      "
                    />
                  </CommandItem>
                </CommandGroup>
              </CommandList>
            </Command>
          </PopoverContent>
        </Popover>
      </div>
      <DialogFooter>
        <Button type="submit"> Create </Button>
      </DialogFooter>
    </DialogContent>
  </Dialog>

  <Table>
    <TableCaption>A list of your created keys</TableCaption>
    <TableHeader>
      <TableRow>
        <TableHead>Name</TableHead>
        <TableHead>Themes</TableHead>
        <TableHead>Actions</TableHead>
      </TableRow>
    </TableHeader>
    <TableBody>
      <TableRow v-for="key in keys">
        <TableCell>{{ key.name }}</TableCell>
        <TableCell>
          <ThemeCollapsible :msg="'wtf'" />
        </TableCell>
        <TableCell><Button variant="destructive" @click=""> Delete </Button></TableCell>
      </TableRow>
    </TableBody>
  </Table>
</template>
