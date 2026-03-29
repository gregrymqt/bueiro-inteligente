/**
 * Form Components Export
 * 
 * Composition Pattern (Recommended):
 * import { Form } from '@/components/Form';
 * <Form onSubmit={...}>
 *   <Form.Input name="email" label="Email" />
 *   <Form.Submit>Enviar</Form.Submit>
 * </Form>
 * 
 * Config-based (Legacy support):
 * import { GenericForm } from '@/components/Form';
 * <GenericForm fields={[...]} onSubmit={...} />
 */

// Composition Pattern (New - Recommended)
export { 
  Form,
  FormInput,
  FormSelect,
  FormTextarea,
  FormCheckbox,
  FormActions,
  FormSubmit
} from './Form';

// Config-based Pattern (Legacy)
export { GenericForm } from './GenericForm';
export type { FormField, InputType } from './GenericForm';