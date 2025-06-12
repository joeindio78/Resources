import { useNavigate, useParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Typography,
  Grid,
} from '@mui/material';
import { Edit as EditIcon } from '@mui/icons-material';
import { getResource } from '@/services/api';

function ResourceDetailsPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const { data: resource, isLoading } = useQuery({
    queryKey: ['resource', id],
    queryFn: () => getResource(Number(id)),
    enabled: !!id,
  });

  if (isLoading) {
    return <Typography>Carregando...</Typography>;
  }

  if (!resource) {
    return <Typography>Recurso não encontrado</Typography>;
  }

  return (
    <Box>
      <Box sx={{ mb: 3, display: 'flex', justifyContent: 'space-between' }}>
        <Typography variant="h4">Detalhes do Recurso</Typography>
        <Button
          variant="contained"
          startIcon={<EditIcon />}
          onClick={() => navigate(`/resources/${id}/edit`)}
        >
          Editar
        </Button>
      </Box>

      <Card>
        <CardContent>
          <Grid container spacing={2}>
            <Grid item xs={12}>
              <Typography variant="h5" gutterBottom>
                {resource.name}
              </Typography>
            </Grid>

            <Grid item xs={12} sm={6}>
              <Typography variant="subtitle1" color="textSecondary">
                Anos de Experiência
              </Typography>
              <Typography variant="body1">
                {resource.yearsOfExperience} anos
              </Typography>
            </Grid>

            {resource.birthDate && (
              <Grid item xs={12} sm={6}>
                <Typography variant="subtitle1" color="textSecondary">
                  Data de Nascimento
                </Typography>
                <Typography variant="body1">{resource.birthDate}</Typography>
              </Grid>
            )}

            <Grid item xs={12}>
              <Typography variant="subtitle1" color="textSecondary" gutterBottom>
                Competências
              </Typography>
              <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
                {resource.competencies.map((competency) => (
                  <Chip
                    key={competency.id}
                    label={competency.name}
                    variant="outlined"
                  />
                ))}
              </Box>
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      <Box sx={{ mt: 2 }}>
        <Button
          variant="outlined"
          onClick={() => navigate('/resources')}
          fullWidth
        >
          Voltar para Lista
        </Button>
      </Box>
    </Box>
  );
}

export default ResourceDetailsPage; 