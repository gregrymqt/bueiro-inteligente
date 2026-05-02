/**
 * Form Components Export
 * 
 * Composition Pattern (Recommended):
 * import { Form } from '@/components/Form';
 * <Form onSubmit={...}>
 *   <Form.Input name="email" label="Email" />
 *   <Form.Submit>Enviar</Form.Submit>
 * </Form>
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
